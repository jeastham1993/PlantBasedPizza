using Dapr.Client;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.PublicEvents;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure;

[AsyncApi]
public class OrderEventPublisher(DaprClient daprClient) : IOrderEventPublisher
{
    private const string SOURCE = "orders";
    private const string PUB_SUB_NAME = "publiC";
    
    [Channel("order.orderSubmitted.v1")]
    [PublishOperation(typeof(OrderSubmittedEventV1), Summary = "Published when an order is submitted.")]
    public async Task PublishOrderSubmittedEventV1(OrderSubmittedEventV1 evt)
    {
        var eventMetadata = new Dictionary<string, string>(2)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, "order.orderSubmitted.v1"},
            { EventConstants.EVENT_ID_HEADER_KEY, Guid.NewGuid().ToString() }
        };
        
        await daprClient.PublishEventAsync(PUB_SUB_NAME, $"{evt.EventName}.{evt.EventVersion}", evt, eventMetadata);
    }

    [Channel("order.orderCompleted.v1")]
    [PublishOperation(typeof(OrderCompletedIntegrationEventV1), Summary = "Published when an order is completed.")]
    public async Task PublishOrderCompletedEventV1(OrderCompletedIntegrationEventV1 evt)
    {
        var eventMetadata = new Dictionary<string, string>(2)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, "order.orderCompleted.v1"},
            { EventConstants.EVENT_ID_HEADER_KEY, Guid.NewGuid().ToString() }
        };
        
        await daprClient.PublishEventAsync(PUB_SUB_NAME, $"{evt.EventName}.{evt.EventVersion}", evt, eventMetadata);
    }

    [Channel("order.readyForDelivery.v1")]
    [PublishOperation(typeof(OrderReadyForDeliveryEventV1),
        Summary = "Published when a delivery order is ready for delivery.")]
    public async Task PublishOrderReadyForDeliveryEventV1(OrderReadyForDeliveryEventV1 evt)
    {
        var eventMetadata = new Dictionary<string, string>(2)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, "order.readyForDelivery.v1"},
            { EventConstants.EVENT_ID_HEADER_KEY, Guid.NewGuid().ToString() }
        };
        await daprClient.PublishEventAsync(PUB_SUB_NAME, $"{evt.EventName}.{evt.EventVersion}", evt, eventMetadata);
    }

    [Channel("order.orderConfirmed.v1")]
    [PublishOperation(typeof(OrderConfirmedEventV1), Summary = "Published when an order is fully confirmed.")]
    public async Task PublishOrderConfirmedEventV1(OrderConfirmedEventV1 evt)
    {
        var eventMetadata = new Dictionary<string, string>(2)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, "order.orderConfirmed.v1"},
            { EventConstants.EVENT_ID_HEADER_KEY, Guid.NewGuid().ToString() }
        };
        await daprClient.PublishEventAsync(PUB_SUB_NAME, $"{evt.EventName}.{evt.EventVersion}", evt, eventMetadata);
    }

    [Channel("order.orderCreated.v1")]
    [PublishOperation(typeof(OrderCreatedEventV1), Summary = "Published when an order is first created.")]
    public async Task PublishOrderCreatedEventV1(OrderCreatedEventV1 evt)
    {
        var eventMetadata = new Dictionary<string, string>(2)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, "order.orderCreated.v1"},
            { EventConstants.EVENT_ID_HEADER_KEY, Guid.NewGuid().ToString() }
        };
        await daprClient.PublishEventAsync(PUB_SUB_NAME, $"{evt.EventName}.{evt.EventVersion}", evt, eventMetadata);
    }
}