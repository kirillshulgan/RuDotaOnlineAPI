using Microsoft.EntityFrameworkCore;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Aggregators;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;
using RuDotaOnlineAPI.Storage.SDK;

namespace RuDotaOnlineAPI.Storage.Identity.Aggregators;

public sealed class ApplicationUserAggregator(IdentityUnitOfWork write,
    IdentityReadUnitOfWork read,
    IdentityAsyncReadUnitOfWork asyncRead)
    : Aggregator<IdentityUnitOfWork, IdentityReadUnitOfWork, IdentityAsyncReadUnitOfWork>(write, read, asyncRead),
      IApplicationUserAggregator
{
    public async Task<ApplicationUser?> GetUserWithRolesAsync(Guid userId, CancellationToken ct = default)
    {
        return await GetCollection<ApplicationUser>()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }
}
