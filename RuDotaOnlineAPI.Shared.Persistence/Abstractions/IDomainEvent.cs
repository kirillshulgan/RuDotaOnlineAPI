using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuDotaOnlineAPI.Shared.Persistence.Abstractions;

/// <summary>
/// Маркерный интерфейс для доменных событий.
/// Реализует INotification — MediatR доставляет их через
/// DomainEventDispatcher после коммита транзакции.
///
/// Важно: доменные события диспатчатся ПОСЛЕ SaveChangesAsync —
/// это гарантирует что данные уже персистированы когда
/// обработчики события начинают работу.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}