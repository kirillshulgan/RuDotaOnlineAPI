using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RuDotaOnlineAPI.Storage.SDK.Abstraction;

namespace RuDotaOnlineAPI.Storage.SDK;

/// <summary>
/// Базовый DbContext для асинхронных read UnitOfWork-ов (AsyncRead replica).
/// Используется для аналитики, отчётов, справочников.
/// </summary>
public abstract class AsyncReadUnitOfWork : DbContext, IAsyncReadUnitOfWork
{
    protected AsyncReadUnitOfWork(DbContextOptions options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }
}