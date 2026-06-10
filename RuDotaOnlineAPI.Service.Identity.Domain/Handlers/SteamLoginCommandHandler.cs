using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Commands;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Handlers;

public sealed class SteamLoginCommandHandler : IRequestHandler<SteamLoginCommand, TokenPairModel>
{
    private readonly ISteamAuthService _steam;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ITokenService _tokens;
    private readonly ILogger<SteamLoginCommandHandler> _logger;

    public SteamLoginCommandHandler(
        ISteamAuthService steam,
        UserManager<ApplicationUser> users,
        ITokenService tokens,
        ILogger<SteamLoginCommandHandler> logger)
    {
        _steam = steam;
        _users = users;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<TokenPairModel> Handle(SteamLoginCommand request, CancellationToken ct)
    {
        var steamModel = await _steam.ValidateCallbackAsync(request.CallbackParams, ct);

        // Ищем существующего пользователя по SteamId
        var user = await _users.FindByLoginAsync("Steam", steamModel.SteamId);

        if (user is null)
        {
            // Первый вход через Steam — создаём пользователя
            user = new ApplicationUser
            {
                UserName = $"steam_{steamModel.SteamId}",
                SteamId = steamModel.SteamId,
                DisplayName = steamModel.DisplayName,
                AvatarUrl = steamModel.AvatarUrl,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false,
            };

            var result = await _users.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Steam user creation failed: {errors}");
            }

            await _users.AddLoginAsync(user, new UserLoginInfo("Steam", steamModel.SteamId, "Steam"));
            await _users.AddToRoleAsync(user, "User");
            _logger.LogInformation("New Steam user registered: {SteamId}", steamModel.SteamId);
        }
        else
        {
            // Обновляем профиль при каждом входе
            user.DisplayName = steamModel.DisplayName;
            user.AvatarUrl = steamModel.AvatarUrl;
            user.LastLoginAt = DateTime.UtcNow;
            await _users.UpdateAsync(user);
        }

        var roles = await _users.GetRolesAsync(user);
        return await _tokens.GenerateTokenPairAsync(user, roles, ct);
    }
}