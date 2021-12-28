using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
                Data = JsonConvert.SerializeObject(evt),
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
                Console.WriteLine(JsonConvert.SerializeObject(cloudEvent));
                foreach (var action in _actions)
                {
                    if (action is Action<T>)
                        ((Action<T>)action)(evt);
                }
            }
        }
    }
}