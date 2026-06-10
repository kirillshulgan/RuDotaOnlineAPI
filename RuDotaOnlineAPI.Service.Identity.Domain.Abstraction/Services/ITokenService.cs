using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;

public interface ITokenService
{
    Task<TokenPairModel> GenerateTokenPairAsync(
        ApplicationUser user,
        IEnumerable<string> roles,
        CancellationToken ct = default);

    Task<TokenPairModel> RefreshAsync(
        string refreshToken,
        CancellationToken ct = default);

    Task RevokeAsync(
        string refreshToken,
        CancellationToken ct = default);

    Task RevokeAllForUserAsync(
        Guid userId,
        CancellationToken ct = default);
}