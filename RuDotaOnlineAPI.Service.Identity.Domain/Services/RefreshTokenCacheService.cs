using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;
using StackExchange.Redis;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Services;

public sealed class RefreshTokenCacheService : IRefreshTokenCacheService
{
    private readonly IDatabase _db;
    private readonly IServer _server;
    private readonly string _prefix;
    private readonly ILogger<RefreshTokenCacheService> _logger;

    public RefreshTokenCacheService(
        IConnectionMultiplexer redis,
        IConfiguration configuration,
        ILogger<RefreshTokenCacheService> logger)
    {
        _db = redis.GetDatabase();
        _server = redis.GetServer(redis.GetEndPoints().First());
        _prefix = configuration["Cache:RefreshTokenPrefix"] ?? "rt";
        _logger = logger;
    }

    private string Key(Guid userId, string tokenId) => $"{_prefix}:{userId}:{tokenId}";
    private string Pattern(Guid userId) => $"{_prefix}:{userId}:*";

    public Task SaveAsync(Guid userId, string tokenId, string tokenHash, TimeSpan ttl, CancellationToken ct = default)
        => _db.StringSetAsync(Key(userId, tokenId), tokenHash, ttl);

    public async Task<string?> GetAsync(Guid userId, string tokenId, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(Key(userId, tokenId));
        return value.HasValue ? value.ToString() : null;
    }

    public Task RemoveAsync(Guid userId, string tokenId, CancellationToken ct = default)
        => _db.KeyDeleteAsync(Key(userId, tokenId));

    public async Task RemoveAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var keys = _server.Keys(pattern: Pattern(userId)).ToArray();
        if (keys.Length == 0) return;

        await _db.KeyDeleteAsync(keys);
        _logger.LogWarning(
            "Reuse detection: revoked {Count} refresh tokens for user {UserId}",
            keys.Length, userId);
    }
}