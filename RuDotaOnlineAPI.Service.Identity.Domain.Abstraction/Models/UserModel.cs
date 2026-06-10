namespace RuDotaOnlineAPI.Service.Identity.Domain.Abstraction.Models;

public sealed record UserModel(
    Guid Id,
    string Email,
    string? DisplayName,
    string? SteamId,
    string? AvatarUrl,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt,
    DateTime? LastLoginAt);