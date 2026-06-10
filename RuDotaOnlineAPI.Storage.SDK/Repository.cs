using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RuDotaOnlineAPI.Storage.SDK.Abstraction;

namespace RuDotaOnlineAPI.Storage.SDK;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> DbSet;
    private readonly ISpecificationEvaluator _evaluator;

    public Repository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
        _evaluator = SpecificationEvaluator.Default;
    }

    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
        => await _evaluator.GetQuery(DbSet.AsQueryable(), spec).FirstOrDefaultAsync(ct);

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken ct = default)
        => await _evaluator.GetQuery(DbSet.AsQueryable(), spec).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default)
        => await _evaluator.GetQuery(DbSet.AsQueryable(), spec).ToListAsync(ct);

    public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken ct = default)
        => await _evaluator.GetQuery(DbSet.AsQueryable(), spec).ToListAsync(ct);

    public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default)
        => await _evaluator.GetQuery(DbSet.AsQueryable(), spec).CountAsync(ct);

    public async Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default)
        => await _evaluator.GetQuery(DbSet.AsQueryable(), spec).AnyAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await DbSet.AddRangeAsync(entities, ct);

    public void Update(T entity) => DbSet.Update(entity);
    public void Remove(T entity) => DbSet.Remove(entity);
    public void RemoveRange(IEnumerable<T> entities) => DbSet.RemoveRange(entities);
}