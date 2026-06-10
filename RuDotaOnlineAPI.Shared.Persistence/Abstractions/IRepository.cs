using System;
using System.Collections.Generic;
using System.Text;

namespace RuDotaOnlineAPI.Shared.Persistence.Abstractions;

/// <summary>
/// Базовый репозиторий для агрегатов.
/// Намеренно минималистичный — только операции которые нужны всегда.
/// Сервисно-специфичные методы добавляются в производных интерфейсах
/// рядом с конкретным агрегатом (IUserRepository, IMatchRepository и т.д.)
/// </summary>
public interface IRepository<T> where T : AggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}