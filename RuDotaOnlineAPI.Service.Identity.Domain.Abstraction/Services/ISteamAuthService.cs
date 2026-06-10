using RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;

namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Services;

public interface ISteamAuthService
{
    /// <summary>Формирует URL редиректа на Steam OpenID.</summary>
    string BuildRedirectUrl();

    /// <summary>Валидирует callback от Steam и возвращает профиль.</summary>
    Task<SteamAuthModel> ValidateCallbackAsync(
        IReadOnlyDictionary<string, string> queryParams,
        CancellationToken ct = default);
}