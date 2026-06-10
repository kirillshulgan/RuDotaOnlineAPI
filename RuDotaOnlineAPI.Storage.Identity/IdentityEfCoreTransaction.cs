using Microsoft.EntityFrameworkCore.Storage;
using RuDotaOnlineAPI.Storage.SDK.Abstraction;

namespace RuDotaOnlineAPI.Storage.Identity;

internal sealed class IdentityEfCoreTransaction : ITransaction
{
    private readonly IDbContextTransaction _tx;
    public IdentityEfCoreTransaction(IDbContextTransaction tx) => _tx = tx;
    public Task CommitAsync(CancellationToken ct = default) => _tx.CommitAsync(ct);
    public Task RollbackAsync(CancellationToken ct = default) => _tx.RollbackAsync(ct);
    public ValueTask DisposeAsync() => _tx.DisposeAsync();
}