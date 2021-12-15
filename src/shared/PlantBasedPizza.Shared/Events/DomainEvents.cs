using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Shared.Events
{
    public static class DomainEvents
    {
        [ThreadStatic] private static List<Delegate> actions;
        
        public static IServiceProvider Container { get; set; }

        public static void Register<T>(Action<T> callback) where T : IDomainEvent
        {
            if (actions == null)
            {
                actions = new List<Delegate>();
            }

            actions.Add(callback);
        }

        public static void ClearCallbacks()
        {
            actions = null;
        }

        public async static Task Raise<T>(T evt) where T : IDomainEvent
        {
            if (Container != null)
            {
                var observability = Container.GetService<IObservabilityService>();
                observability.Info(evt.CorrelationId, $"[EVENT MANAGER] Raising event {evt.EventName}");
                
                foreach (var handler in Container.GetServices<Handles<T>>())
                {
                    observability.Info(evt.CorrelationId, $"[EVENT MANAGER] Handling event with handler {handler.GetType().Name}");
                    await handler.Handle(evt);
                }
            }

            if (actions != null)
            {
                foreach (var action in actions)
                {
                    if (action is Action<T>)
                        ((Action<T>)action)(evt);
                }
            }
        }
    }
}