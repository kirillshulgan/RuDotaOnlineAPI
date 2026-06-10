namespace RuDotaOnlineAPI.Shared.Persistence.Abstractions;

/// <summary>
/// Unit of Work — управление транзакцией явно в handler-ах.
///
/// Паттерн использования в CommandHandler:
///
///   await using var tx = await _uow.BeginTransactionAsync(ct);
///   try
///   {
///       // ... бизнес-логика, изменение агрегатов ...
///       await _uow.SaveChangesAsync(ct);  // персистируем + диспатчим доменные события
///       await tx.CommitAsync(ct);
///   }
///   catch
///   {
///       await tx.RollbackAsync(ct);
///       throw;
///   }
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Сохраняет изменения и диспатчит доменные события всех агрегатов.
    /// Доменные события диспатчатся ПОСЛЕ записи в БД.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Открывает явную транзакцию БД. Используется когда нужна
    /// атомарность между несколькими операциями SaveChanges,
    /// или совместно с MassTransit Outbox (одна транзакция).
    /// </summary>
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default);
}