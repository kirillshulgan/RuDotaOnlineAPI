using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RuDotaOnlineAPI.Storage.SDK.Abstraction;

namespace RuDotaOnlineAPI.Storage.SDK;

/// <summary>
/// Базовый DbContext для синхронных read UnitOfWork-ов (SyncRead replica).
/// NoTracking по умолчанию — read-only контекст.
/// </summary>
public abstract class ReadUnitOfWork : DbContext, IReadUnitOfWork
{
    protected ReadUnitOfWork(DbContextOptions options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }
}