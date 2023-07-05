using System.Text.Json;
using CloudNative.CloudEvents;
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
            var cloudEvent = new CloudEvent()
            {
                Type = evt.EventName,
                Source = new Uri("https://github.com/cloudevents/sdk-csharp"),
                Time = evt.EventDate,
                DataContentType = "application/json",
                Id = evt.EventId,
                Data = JsonSerializer.Serialize(evt),
            };

            if (Container != null)
            {
                var observability = Container.GetService<IObservabilityService>();
                
                observability?.Info($"[EVENT MANAGER] Raising event {evt.EventName}");

                foreach (var handler in Container.GetServices<Handles<T>>())
                {
                    observability?.StartTraceSubsegment(handler.GetType().Name);
                    
                    observability?.Info($"[EVENT MANAGER] Handling event with handler {handler.GetType().Name}");
                    
                    await handler.Handle(evt);
                    
                    observability?.EndTraceSubsegment();
                }
            }

            if (_actions != null)
            {
                Console.WriteLine(JsonSerializer.Serialize(cloudEvent));
                foreach (var action in _actions)
                {
                    if (action is Action<T>)
                        ((Action<T>)action)(evt);
                }
            }
        }
    }
}