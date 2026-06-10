using Microsoft.EntityFrameworkCore.Storage;
using RuDotaOnlineAPI.Shared.Persistence.Abstractions;

namespace RuDotaOnlineAPI.Shared.Persistence.EfCore;

/// <summary>
/// Адаптер EF Core IDbContextTransaction → наш IDbTransaction.
/// </summary>
internal sealed class EfCoreTransaction : IDbTransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfCoreTransaction(IDbContextTransaction transaction)
        => _transaction = transaction;

    public Task CommitAsync(CancellationToken ct = default)
        => _transaction.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default)
        => _transaction.RollbackAsync(ct);

    public ValueTask DisposeAsync()
        => _transaction.DisposeAsync();
}