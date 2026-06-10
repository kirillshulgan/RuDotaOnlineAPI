using System;
using System.Collections.Generic;
using System.Text;

namespace RuDotaOnlineAPI.Shared.Persistence.Abstractions;

/// <summary>
/// Корень агрегата — сущность, которая является точкой входа
/// для всех операций над агрегатом и единственным владельцем
/// доменных событий агрегата.
///
/// Доменные события накапливаются в памяти (_domainEvents)
/// и диспатчатся через DomainEventDispatcher после
/// успешного SaveChangesAsync в BaseDbContext.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}