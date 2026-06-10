using Ardalis.Specification;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Storage.Identity.Specifications;

public sealed class ApplicationUserByEmailSpec : SingleResultSpecification<ApplicationUser>
{
    public ApplicationUserByEmailSpec(string email)
        => Query.Where(u => u.Email == email && u.EmailConfirmed || u.NormalizedEmail == email.ToUpper());
}
