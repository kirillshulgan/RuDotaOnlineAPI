using Ardalis.Specification;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Storage.Identity.Specifications;

public sealed class ApplicationUserBySteamIdSpec : SingleResultSpecification<ApplicationUser>
{
    public ApplicationUserBySteamIdSpec(string steamId)
        => Query.Where(u => u.SteamId == steamId);
}
