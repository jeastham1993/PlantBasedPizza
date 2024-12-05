using Dapr.Client;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.PublicEvents;
using PlantBasedPizza.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.Deliver.Infrastructure;

[AsyncApi]
public class DeliveryEventPublisher(DaprClient daprClient) : IDeliveryEventPublisher
{
    private const string SOURCE = "delivery";
    private const string PUB_SUB_NAME = "public";
    private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";
    
    [Channel("delivery.driverCollectedOrder.v1")]
    [SubscribeOperation(typeof(DriverCollectedOrderEventV1), OperationId = nameof(DriverCollectedOrderEventV1), Summary = "Published when a driver collects an order.")]
    public async Task PublishDriverOrderCollectedEventV1(DriverCollectedOrderEventV1 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();
        
        evt.AddToTelemetry(eventId);
        
        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType},
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DATE_FORMAT) },
        };
        
        await daprClient.PublishEventAsync(PUB_SUB_NAME, eventType, evt, eventMetadata);
    }

    [Channel("delivery.driverDeliveredOrder.v1")]
    [SubscribeOperation(typeof(DriverDeliveredOrderEventV1), OperationId = nameof(DriverDeliveredOrderEventV1), Summary = "Published when a driver delivers an order.")]
    public async Task PublishDriverDeliveredOrderEventV1(DriverDeliveredOrderEventV1 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();
        
        evt.AddToTelemetry(eventId);
        
        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType},
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DATE_FORMAT) },
        };
        
        await daprClient.PublishEventAsync(PUB_SUB_NAME, eventType, evt, eventMetadata);
    }
}