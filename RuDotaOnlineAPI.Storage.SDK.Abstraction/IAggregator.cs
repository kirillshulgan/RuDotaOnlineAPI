namespace RuDotaOnlineAPI.Storage.SDK.Abstraction;

public interface IAggregator<TWrite, TRead, TAsyncRead>
    where TWrite : IUnitOfWork
    where TRead : IReadUnitOfWork
    where TAsyncRead : IAsyncReadUnitOfWork
{
}