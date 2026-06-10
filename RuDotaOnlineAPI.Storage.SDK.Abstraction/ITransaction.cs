namespace RuDotaOnlineAPI.Storage.SDK.Abstraction;

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}