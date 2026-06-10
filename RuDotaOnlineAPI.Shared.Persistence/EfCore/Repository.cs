using Microsoft.EntityFrameworkCore;
using RuDotaOnlineAPI.Shared.Persistence.Abstractions;

namespace RuDotaOnlineAPI.Shared.Persistence.EfCore;

/// <summary>
/// Дефолтная EF Core реализация IRepository&lt;T&gt;.
/// Регистрируется в DI как открытый generic:
///   services.AddScoped(typeof(IRepository&lt;&gt;), typeof(Repository&lt;&gt;))
///
/// Для агрегатов с кастомными запросами создаётся производный класс:
///   public class UserRepository : Repository&lt;ApplicationUser&gt;, IUserRepository
/// </summary>
public class Repository<T> : IRepository<T> where T : AggregateRoot
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);

    public void Update(T entity)
        => DbSet.Update(entity);

    public void Remove(T entity)
        => DbSet.Remove(entity);
}