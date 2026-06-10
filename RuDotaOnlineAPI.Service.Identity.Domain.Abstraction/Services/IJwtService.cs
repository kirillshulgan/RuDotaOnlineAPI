using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;
using System.Security.Claims;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    ClaimsPrincipal? ValidateAccessToken(string token);

    /// <summary>Возвращает JWKS JSON для публичного endpoint-а Gateway.</summary>
    string GetJwks();
}