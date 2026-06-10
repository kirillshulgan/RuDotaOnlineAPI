using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;
using RuDotaOnlineAPI.Storage.SDK.Abstraction;

namespace RuDotaOnlineAPI.Storage.Identity.Abstraction.Repositories;

public interface IApplicationUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<ApplicationUser?> GetBySteamIdAsync(string steamId, CancellationToken ct = default);
}