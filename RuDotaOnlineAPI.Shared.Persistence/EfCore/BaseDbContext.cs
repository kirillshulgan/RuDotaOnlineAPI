using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RuDotaOnlineAPI.Shared.Persistence.Abstractions;
using RuDotaOnlineAPI.Shared.Persistence.Dispatching;

namespace RuDotaOnlineAPI.Shared.Persistence.EfCore;

/// <summary>
/// Базовый DbContext для всех сервисов системы.
/// Добавляет:
///   1. Автоматический диспатч доменных событий после SaveChanges
///   2. Реализацию IUnitOfWork
///
/// Использование в сервисе:
///   public class AppDbContext : BaseDbContext&lt;AppDbContext&gt; { ... }
///
/// Для Identity.API: AppDbContext наследует IdentityDbContext,
/// поэтому там BaseDbContext не используется как базовый класс —
/// вместо этого IUnitOfWork и DomainEventDispatcher регистрируются
/// через AppDbContextUnitOfWork-адаптер (см. ниже).
/// </summary>
public abstract class BaseDbContext<TContext> : DbContext, IUnitOfWork
    where TContext : DbContext
{
    private readonly DomainEventDispatcher _dispatcher;

    protected BaseDbContext(
        DbContextOptions<TContext> options,
        DomainEventDispatcher dispatcher)
        : base(options)
    {
        _dispatcher = dispatcher;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // 1. Собираем агрегаты с событиями до сохранения
        var aggregatesWithEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        // 2. Сохраняем изменения в БД
        var result = await base.SaveChangesAsync(ct);

        // 3. Диспатчим доменные события ПОСЛЕ сохранения
        if (aggregatesWithEvents.Count > 0)
            await _dispatcher.DispatchAsync(aggregatesWithEvents, ct);

        return result;
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var transaction = await Database.BeginTransactionAsync(ct);
        return new EfCoreTransaction(transaction);
    }
}