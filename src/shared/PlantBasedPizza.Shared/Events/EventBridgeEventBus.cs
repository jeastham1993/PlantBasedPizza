using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.Logging;

namespace PlantBasedPizza.Shared.Events;

internal class EventBridgeEventBus : IEventBus
{
    private readonly ILogger<EventBridgeEventBus> _logger;
    private readonly AmazonEventBridgeClient _eventBridgeClient;
    
    public EventBridgeEventBus(ILogger<EventBridgeEventBus> logger, AmazonEventBridgeClient eventBridgeClient)
    {
        _logger = logger;
        _eventBridgeClient = eventBridgeClient;
    }
    
    public async Task Publish(IntegrationEvent evt)
    {
        this._logger.LogInformation($"Publishing {evt.EventName}");
        
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