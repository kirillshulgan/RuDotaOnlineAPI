using Microsoft.EntityFrameworkCore;
using RuDotaOnlineAPI.Storage.SDK.Abstraction;

namespace RuDotaOnlineAPI.Storage.SDK;

/// <summary>
/// Базовый агрегатор — собирает данные из нескольких DbSet-ов
/// через один из трёх контекстов (Leader / SyncRead / AsyncRead).
///
/// GetCollection&lt;T&gt;() возвращает IQueryable из SyncRead контекста по умолчанию.
/// GetLeaderCollection&lt;T&gt;() — из Leader (для критичных чтений после записи).
/// GetAsyncCollection&lt;T&gt;() — из AsyncRead (для аналитики).
/// </summary>
public abstract class Aggregator<TWrite, TRead, TAsyncRead>
    : IAggregator<TWrite, TRead, TAsyncRead>
    where TWrite : DbContext, IUnitOfWork
    where TRead : DbContext, IReadUnitOfWork
    where TAsyncRead : DbContext, IAsyncReadUnitOfWork
{
    protected readonly TWrite WriteContext;
    protected readonly TRead ReadContext;
    protected readonly TAsyncRead AsyncReadContext;

    protected Aggregator(TWrite write, TRead read, TAsyncRead asyncRead)
    {
        WriteContext = write;
        ReadContext = read;
        AsyncReadContext = asyncRead;
    }

    /// <summary>SyncRead replica — обычные запросы чтения.</summary>
    protected IQueryable<T> GetCollection<T>() where T : class
        => ReadContext.Set<T>().AsNoTracking();

    /// <summary>Leader — критичные чтения (сразу после записи).</summary>
    protected IQueryable<T> GetLeaderCollection<T>() where T : class
        => WriteContext.Set<T>().AsNoTracking();

    /// <summary>AsyncRead replica — аналитика, справочники.</summary>
    protected IQueryable<T> GetAsyncCollection<T>() where T : class
        => AsyncReadContext.Set<T>().AsNoTracking();
}