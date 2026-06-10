namespace RuDotaOnlineAPI.Gateway.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// URI к JWKS-эндпоинту Identity.API (/.well-known/jwks.json).
    /// Gateway автоматически подтягивает публичные ключи — ротация без деплоя.
    /// </summary>
    public string JwksUri { get; init; } = string.Empty;
}