using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Storage.Identity.Abstraction.Aggregators;

public interface IApplicationUserAggregator
{
    Task<ApplicationUser?> GetUserWithRolesAsync(Guid userId, CancellationToken ct = default);
}