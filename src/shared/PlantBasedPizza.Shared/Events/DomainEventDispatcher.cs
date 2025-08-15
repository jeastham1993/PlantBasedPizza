using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Shared.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;
    private readonly ActivitySource? _activitySource;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider, 
        ILogger<DomainEventDispatcher> logger,
        ActivitySource? activitySource = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activitySource = activitySource;
    }

    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) 
        where T : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _logger.LogDebug("Publishing domain event {EventType} with ID {EventId}", 
            typeof(T).Name, domainEvent.EventId);

        Activity.Current?.SetTag("events.eventId", domainEvent.EventId);
        Activity.Current?.SetTag("events.eventName", domainEvent.EventName);
        Activity.Current?.SetTag("events.eventVersion", domainEvent.EventVersion);
        Activity.Current?.SetTag("correlationId", domainEvent.CorrelationId);

        using var serviceScope = _serviceProvider.CreateScope();
        using var sendSpan = _activitySource?.StartActivity($"send {domainEvent.EventName}", ActivityKind.Producer);
        
        var hasErrors = false;
        var handlers = serviceScope.ServiceProvider.GetServices<Handles<T>>();

        foreach (var handler in handlers)
        {
            using var span = _activitySource?.StartActivity($"process {domainEvent.EventName}", ActivityKind.Consumer);
            
            span?.AddTag("messaging.operation.name", "process");
            span?.AddTag("messaging.system", "in_memory");
            span?.AddTag("messaging.consumer.group.name", $"{handler.GetType().Namespace}.{handler.GetType().Name}");
            span?.AddTag("messaging.message.id", domainEvent.EventId);
            span?.AddTag("messaging.message.body.size", domainEvent.ToString()?.Length ?? 0);
            span?.AddTag("messaging.message.conversation_id", domainEvent.CorrelationId);

            try
            {
                await handler.Handle(domainEvent);
                _logger.LogDebug("Handler {HandlerType} processed event {EventType} successfully", 
                    handler.GetType().Name, typeof(T).Name);
            }
            catch (Exception ex)
            {
                hasErrors = true;
                span?.AddTag("error.type", ex.GetType().Name);
                span?.AddException(ex);
                _logger.LogError(ex, "Handler {HandlerType} failed to process event {EventType}", 
                    handler.GetType().Name, typeof(T).Name);
            }
        }

        if (hasErrors)
        {
            throw new InvalidOperationException("One or more event handlers failed to process the event.");
        }

        _logger.LogDebug("Successfully published domain event {EventType} to {HandlerCount} handlers", 
            typeof(T).Name, handlers.Count());
    }
}