using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;

namespace PlantBasedPizza.Shared.Events;

internal class EventBridgeEventBus : IEventBus
{
    private readonly AmazonEventBridgeClient _eventBridgeClient;
    
    public EventBridgeEventBus(AmazonEventBridgeClient eventBridgeClient)
    {
        _eventBridgeClient = eventBridgeClient;
    }
    
    public async Task Publish(IntegrationEvent evt)
    {
        await this._eventBridgeClient.PutEventsAsync(new PutEventsRequest()
        {
            Entries = new List<PutEventsRequestEntry>(1)
            {
                new PutEventsRequestEntry()
                {
                    Detail = JsonSerializer.Serialize<object>(evt),
                    DetailType = evt.EventName,
                    Source = "com.order-service"
                }
            }
        });
    }
}