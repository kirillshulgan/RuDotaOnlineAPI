using Microsoft.AspNetCore.Identity;

namespace RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? SteamId { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}