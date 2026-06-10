using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;
using System.Security.Cryptography;
using System.Text;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Services;

public sealed class TokenService : ITokenService
{
    private readonly IJwtService _jwt;
    private readonly IRefreshTokenCacheService _cache;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly int _refreshTtlDays;

    public TokenService(
        IJwtService jwt,
        IRefreshTokenCacheService cache,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _jwt = jwt;
        _cache = cache;
        _userManager = userManager;
        _refreshTtlDays = int.Parse(configuration["Jwt:RefreshTokenTtlDays"] ?? "30");
    }

    public async Task<TokenPairModel> GenerateTokenPairAsync(
        ApplicationUser user,
        IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        var accessToken = _jwt.GenerateAccessToken(user, roles);

        var tokenId = Guid.NewGuid().ToString();
        var rawToken = $"{user.Id}:{tokenId}:{Guid.NewGuid()}";
        var tokenHash = Hash(rawToken);
        var ttl = TimeSpan.FromDays(_refreshTtlDays);

        await _cache.SaveAsync(user.Id, tokenId, tokenHash, ttl, ct);

        // Refresh токен передаётся клиенту в формате base64(userId:tokenId:rawToken)
        var refreshToken = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{user.Id}:{tokenId}:{rawToken}"));

        return new TokenPairModel(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.Add(ttl));
    }

    public async Task<TokenPairModel> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var (userId, tokenId, raw) = ParseRefreshToken(refreshToken);

        var storedHash = await _cache.GetAsync(userId, tokenId, ct);

        if (storedHash is null)
        {
            // Токен не найден — возможен reuse attack, отзываем все токены
            await _cache.RemoveAllForUserAsync(userId, ct);
            throw new UnauthorizedAccessException("Refresh token reuse detected.");
        }

        if (storedHash != Hash(raw))
            throw new UnauthorizedAccessException("Invalid refresh token.");

        // Token Rotation: удаляем старый
        await _cache.RemoveAsync(userId, tokenId, ct);

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAccessException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return await GenerateTokenPairAsync(user, roles, ct);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var (userId, tokenId, _) = ParseRefreshToken(refreshToken);
        await _cache.RemoveAsync(userId, tokenId, ct);
    }

    public Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
        => _cache.RemoveAllForUserAsync(userId, ct);

    // ---------------------------------------------------------------
    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    private static (Guid userId, string tokenId, string raw) ParseRefreshToken(string token)
    {
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':', 3);
            return (Guid.Parse(parts[0]), parts[1], decoded);
        }
        catch
        {
            throw new UnauthorizedAccessException("Malformed refresh token.");
        }
    }
}