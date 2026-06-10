namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;

public sealed record TokenPairModel(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);