using MediatR;
using Microsoft.Extensions.Logging;
using RuDotaOnlineAPI.Shared.Persistence.Abstractions;

namespace RuDotaOnlineAPI.Shared.Persistence.Dispatching;

/// <summary>
/// Собирает доменные события со всех агрегатов в DbContext,
/// очищает их и публикует через MediatR IPublisher.
///
/// Вызывается из BaseDbContext.SaveChangesAsync — после того
/// как EF Core записал изменения в БД, но ещё не закрыл транзакцию
/// (если транзакция явная). Это даёт обработчикам событий
/// доступ к уже сохранённым данным.
///
/// Порядок диспатча: события публикуются в порядке их создания
/// (FIFO по каждому агрегату, порядок между агрегатами не гарантирован).
/// </summary>
public sealed class DomainEventDispatcher
{
    private readonly IPublisher _publisher;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IPublisher publisher, ILogger<DomainEventDispatcher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task DispatchAsync(
        IEnumerable<AggregateRoot> aggregates,
        CancellationToken ct = default)
    {
        // Снимаем события до очистки — на случай если один из обработчиков упадёт
        var events = aggregates
            .SelectMany(a =>
            {
                var domainEvents = a.DomainEvents.ToList();
                a.ClearDomainEvents();
                return domainEvents;
            })
            .ToList();

        if (events.Count == 0) return;

        _logger.LogDebug("Dispatching {Count} domain events: {EventTypes}",
            events.Count,
            string.Join(", ", events.Select(e => e.GetType().Name)));

        foreach (var domainEvent in events)
        {
            try
            {
                await _publisher.Publish(domainEvent, ct);
            }
            catch (Exception ex)
            {
                // Логируем но не прерываем диспатч остальных событий.
                // Падение одного обработчика не должно откатывать
                // уже сохранённые данные.
                _logger.LogError(ex,
                    "Error dispatching domain event {EventType} (EventId={EventId})",
                    domainEvent.GetType().Name,
                    domainEvent.EventId);
            }
        }
    }
}