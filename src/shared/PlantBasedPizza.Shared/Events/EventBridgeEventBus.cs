using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using PlantBasedPizza.Shared.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlantBasedPizza.Shared.Events
{
    public class EventBridgeEventBus : IEventBus
    {
        private readonly AmazonEventBridgeClient _eventBridgeClient;
        private readonly IObservabilityService _observabilityService;

        public EventBridgeEventBus(AmazonEventBridgeClient eventBridgeClient, IObservabilityService observabilityService)
        {
            this._eventBridgeClient = eventBridgeClient;
            this._observabilityService = observabilityService;
        }

        public async Task Publish(BaseEvent evt)
        {
            this._observabilityService.Info($"Publishing event {evt.EventName}");

            await this._eventBridgeClient.PutEventsAsync(new PutEventsRequest()
            {
                Entries = new List<PutEventsRequestEntry>()
                {
                    new PutEventsRequestEntry()
                    {
                        Detail = JsonSerializer.Serialize(evt),
                        DetailType = evt.EventName,
                        Source = evt.Service,
                        TraceHeader = CorrelationContext.GetCorrelationId(),
                        EventBusName = "default"
                    }
                }
            });
        }
    }
}
