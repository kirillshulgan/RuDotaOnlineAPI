namespace RuDotaOnlineAPI.Shared.Persistence.Abstractions;

/// <summary>
/// Обёртка над EF Core IDbContextTransaction для абстрагирования
/// от конкретного провайдера БД.
/// </summary>
public interface IDbTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}