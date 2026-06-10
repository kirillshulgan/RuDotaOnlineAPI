using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RuDotaOnlineAPI.Storage.SDK.Abstraction;

namespace RuDotaOnlineAPI.Storage.SDK;

/// <summary>
/// Базовый DbContext для всех write UnitOfWork-ов.
/// Наследуется сервис-специфичным UnitOfWorkBase:
///   public abstract class IdentityUnitOfWorkBase(DbContextOptions options)
///       : UnitOfWork(options) { ... DbSet-ы ... }
/// </summary>
public abstract class UnitOfWork : DbContext, IUnitOfWork
{
    protected UnitOfWork(DbContextOptions options) : base(options) { }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await base.SaveChangesAsync(ct);

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var tx = await Database.BeginTransactionAsync(ct);
        return new EfCoreTransaction(tx);
    }
}