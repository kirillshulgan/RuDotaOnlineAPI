namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;

public interface IRefreshTokenCacheService
{
    Task SaveAsync(Guid userId, string tokenId, string tokenHash, TimeSpan ttl, CancellationToken ct = default);
    Task<string?> GetAsync(Guid userId, string tokenId, CancellationToken ct = default);
    Task RemoveAsync(Guid userId, string tokenId, CancellationToken ct = default);
    Task RemoveAllForUserAsync(Guid userId, CancellationToken ct = default);
}