using Dapr.Client;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.PublicEvents;
using Saunter.Attributes;

namespace PlantBasedPizza.Kitchen.Infrastructure;

[AsyncApi]
public class KitchenEventPublisher(DaprClient daprClient) : IKitchenEventPublisher
{
    private const string SOURCE = "kitchen";
    private const string PUB_SUB_NAME = "public";
    private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

    [Channel("kitchen.orderConfirmed.v1")]
    [SubscribeOperation(typeof(KitchenConfirmedOrderEventV1), OperationId = nameof(KitchenConfirmedOrderEventV1), Summary = "Published when the kitchen confirms an order.")]
    public async Task PublishKitchenConfirmedOrderEventV1(KitchenConfirmedOrderEventV1 evt)
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

    [Channel("kitchen.orderBaked.v1")]
    [SubscribeOperation(typeof(OrderBakedEventV1), OperationId = nameof(OrderBakedEventV1), Summary = "Published when the kitchen finishes baking an order.")]
    public async Task PublishOrderBakedEventV1(OrderBakedEventV1 evt)
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

    [Channel("kitchen.orderPreparing.v1")]
    [SubscribeOperation(typeof(OrderPreparingEventV1), OperationId = nameof(OrderPreparingEventV1), Summary = "Published when the kitchen starts preparing an order.")]
    public async Task PublishOrderPreparingEventV1(OrderPreparingEventV1 evt)
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

    [Channel("kitchen.orderPrepComplete.v1")]
    [SubscribeOperation(typeof(OrderPreparingEventV1), OperationId = nameof(OrderPrepCompleteEventV1), Summary = "Published when the kitchen finishes preparing an order.")]
    public async Task PublishOrderPrepCompleteEventV1(OrderPrepCompleteEventV1 evt)
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

    [Channel("kitchen.qualityChecked.v1")]
    [SubscribeOperation(typeof(OrderQualityCheckedEventV1), OperationId = nameof(OrderQualityCheckedEventV1), Summary = "Published when the kitchen quality checks an order, this indicates it is ready.")]
    public async Task PublishOrderQualityCheckedEventV1(OrderQualityCheckedEventV1 evt)
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