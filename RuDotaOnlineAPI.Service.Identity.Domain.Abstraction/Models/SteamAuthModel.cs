namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;

public sealed record SteamAuthModel(
    string SteamId,
    string DisplayName,
    string? AvatarUrl);