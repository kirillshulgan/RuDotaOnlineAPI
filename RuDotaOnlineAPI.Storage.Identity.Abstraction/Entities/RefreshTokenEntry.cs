namespace RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

/// <summary>
/// Data model для хранения refresh токенов (если понадобится персистенция в БД,
/// сейчас используется Redis — этот класс как альтернатива/расширение).
/// </summary>
public sealed class RefreshTokenEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
}