using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Shared.Events
{
    public static class DomainEvents
    {
        [ThreadStatic] private static List<Delegate>? _actions;
        
        public static IServiceProvider? Container { get; set; }

        public static void Register<T>(Action<T> callback) where T : IDomainEvent
        {
            if (_actions == null)
            {
                _actions = new List<Delegate>();
            }

            _actions.Add(callback);
        }

        public static void ClearCallbacks()
        {
            _actions = new List<Delegate>();
        }

        public async static Task Raise<T>(T evt) where T : IDomainEvent
        {
            if (Container != null)
            {
                Activity.Current?.SetTag("events.eventId", evt.EventId);
                Activity.Current?.SetTag("events.eventName", evt.EventName);
                Activity.Current?.SetTag("events.eventVersion", evt.EventVersion);
                Activity.Current?.SetTag("correlationId", evt.CorrelationId);
                
                var serviceScopeFactory = Container.GetService<IServiceScopeFactory>();

                if (serviceScopeFactory is null)
                {
                    throw new Exception("Cannot raise domain events, service scope factory is not registered.");
                }
                
                var activitySource = Container.GetService<ActivitySource>();

                using var serviceScope = serviceScopeFactory.CreateScope();
                
                using var sendSpan = activitySource?.StartActivity($"send {evt.EventName}", ActivityKind.Producer);
                    
                var hasErrors = false;

                foreach (var handler in serviceScope.ServiceProvider.GetServices<Handles<T>>())
                {
                    using var span = activitySource?.StartActivity($"process {evt.EventName}", ActivityKind.Consumer);
                        
                    span?.AddTag("messaging.operation.name", "process");
                    span?.AddTag("messaging.system", "in_memory");
                    span?.AddTag("messaging.consumer.group.name", $"{handler.GetType().Namespace}.{handler.GetType().Name}");
                    span?.AddTag("messaging.message.id", evt.EventId);
                    span?.AddTag("messaging.message.body.size", evt.ToString()?.Length ?? 0);
                    span?.AddTag("messaging.message.conversation_id", evt.CorrelationId);

                    try
                    {
                        await handler.Handle(evt);
                    }
                    catch (Exception ex)
                    {
                        hasErrors = true;
                        span?.AddTag("error.type", ex.GetType().Name);
                        span?.AddException(ex);
                    }
                }

                if (hasErrors)
                {
                    throw new Exception("One or more event handlers failed to process the event.");
                }
            }

            if (_actions != null)
            {
                foreach (var action in _actions.Where(action => action is Action<T>))
                {
                    ((Action<T>)action)(evt);
                }
            }
        }
    }
}