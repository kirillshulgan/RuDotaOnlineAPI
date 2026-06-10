using Microsoft.EntityFrameworkCore;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Repositories;
using RuDotaOnlineAPI.Storage.SDK;

namespace RuDotaOnlineAPI.Storage.Identity.Repositories;

public sealed class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
{
    public ApplicationUserRepository(IdentityUnitOfWork context) : base(context) { }

    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        => DbSet.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<ApplicationUser?> GetBySteamIdAsync(string steamId, CancellationToken ct = default)
        => DbSet.FirstOrDefaultAsync(u => u.SteamId == steamId, ct);
}