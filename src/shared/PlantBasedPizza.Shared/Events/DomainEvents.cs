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
                var observability = Container.GetService<IObservabilityService>();
                
                observability?.Info($"[EVENT MANAGER] Raising event {evt.EventName}");

                foreach (var handler in Container.GetServices<Handles<T>>())
                {
                    observability?.Info($"[EVENT MANAGER] Handling event with handler {handler.GetType().Name}");
                    
                    await handler.Handle(evt);
                }
            }

            if (_actions != null)
            {
                foreach (var action in _actions)
                {
                    if (action is Action<T>)
                        ((Action<T>)action)(evt);
                }
            }
        }
    }
}