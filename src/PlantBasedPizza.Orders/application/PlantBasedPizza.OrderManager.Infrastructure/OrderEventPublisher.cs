using System.Diagnostics;
using System.Text.Json;
using Dapr.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.OrderManager.Core.PublicEvents;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure;

[AsyncApi]
public class OrderEventPublisher(
    DaprClient daprClient,
    IConfiguration configuration,
    IOrderRepository repository,
    IDistributedCache cache) : IOrderEventPublisher
{
    private const string Source = "orders";
    private const string PubSubName = "public";
    private const string DateFormat = "yyyy-MM-ddTHH:mm:ssZ";
    private readonly string _clientId = configuration["ApplicationConfig:ApplicationName"] ?? "";

    [Channel("order.orderSubmitted.v1")]
    [SubscribeOperation(typeof(OrderSubmittedEventV1), OperationId = nameof(OrderSubmittedEventV1),
        Summary = "Published when an order is submitted.")]
    public async Task PublishOrderSubmittedEventV1(OrderSubmittedEventV1 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();

        using var activity = Activity.Current?.Source.StartActivityWithSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            eventType,
            eventId,
            "dapr",
            PubSubName,
            configuration[_clientId] ?? "",
            evt.OrderIdentifier
        ));

        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, Source },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType },
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DateFormat) }
        };
        await daprClient.PublishEventAsync(PubSubName, eventType, evt, eventMetadata);
    }

    [Channel("order.orderCompleted.v1")]
    [SubscribeOperation(typeof(OrderCompletedIntegrationEventV1),
        OperationId = nameof(OrderCompletedIntegrationEventV1), Summary = "Published when an order is completed.")]
    [Obsolete("Consumers should migrate to version 2 of the event")]
    public async Task PublishOrderCompletedEventV1(OrderCompletedIntegrationEventV1 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();

        using var activity = Activity.Current?.Source.StartActivityWithSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            eventType,
            eventId,
            "dapr",
            PubSubName,
            configuration[_clientId] ?? "",
            evt.OrderIdentifier
        ));

        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, Source },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType },
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DateFormat) }
        };
        await CacheOrder(evt.OrderIdentifier);
        await daprClient.PublishEventAsync(PubSubName, eventType, evt, eventMetadata);
    }

    [Channel("order.orderCompleted.v2")]
    [SubscribeOperation(typeof(OrderCompletedIntegrationEventV2),
        OperationId = nameof(OrderCompletedIntegrationEventV2), Summary = "Published when an order is completed.")]
    public async Task PublishOrderCompletedEventV2(OrderCompletedIntegrationEventV2 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();

        using var activity = Activity.Current?.Source.StartActivityWithSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            eventType,
            eventId,
            "dapr",
            PubSubName,
            configuration[_clientId] ?? "",
            evt.OrderIdentifier
        ));

        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, Source },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType },
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DateFormat) }
        };
        await CacheOrder(evt.OrderIdentifier);
        await daprClient.PublishEventAsync(PubSubName, eventType, evt, eventMetadata);
    }

    [Channel("order.readyForDelivery.v1")]
    [SubscribeOperation(typeof(OrderReadyForDeliveryEventV1),
        OperationId = nameof(OrderReadyForDeliveryEventV1),
        Summary = "Published when a delivery order is ready for delivery.")]
    public async Task PublishOrderReadyForDeliveryEventV1(OrderReadyForDeliveryEventV1 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();

        using var activity = Activity.Current?.Source.StartActivityWithSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            eventType,
            eventId,
            "dapr",
            PubSubName,
            configuration[_clientId] ?? "",
            evt.OrderIdentifier
        ));

        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, Source },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType },
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DateFormat) }
        };
        await CacheOrder(evt.OrderIdentifier);
        await daprClient.PublishEventAsync(PubSubName, eventType, evt, eventMetadata);
    }

    [Channel("order.orderConfirmed.v1")]
    [SubscribeOperation(typeof(OrderConfirmedEventV1), OperationId = nameof(OrderConfirmedEventV1),
        Summary = "Published when an order is fully confirmed.")]
    public async Task PublishOrderConfirmedEventV1(OrderConfirmedEventV1 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();

        using var activity = Activity.Current?.Source.StartActivityWithSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            eventType,
            eventId,
            "dapr",
            PubSubName,
            configuration[_clientId] ?? "",
            evt.OrderIdentifier
        ));

        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, Source },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType },
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DateFormat) }
        };
        await CacheOrder(evt.OrderIdentifier);
        await daprClient.PublishEventAsync(PubSubName, eventType, evt, eventMetadata);
    }

    [Channel("order.orderCreated.v1")]
    [SubscribeOperation(typeof(OrderCreatedEventV1), OperationId = nameof(OrderCreatedEventV1),
        Summary = "Published when an order is first created.")]
    public async Task PublishOrderCreatedEventV1(OrderCreatedEventV1 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();

        using var activity = Activity.Current?.Source.StartActivityWithSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            eventType,
            eventId,
            "dapr",
            PubSubName,
            configuration[_clientId] ?? "",
            evt.OrderIdentifier
        ));

        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, Source },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType },
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DateFormat) }
        };
        await CacheOrder(evt.OrderIdentifier);
        await daprClient.PublishEventAsync(PubSubName, eventType, evt, eventMetadata);
    }

    [Channel("order.orderCancelled.v1")]
    [SubscribeOperation(typeof(OrderCancelledEventV1), OperationId = nameof(OrderCancelledEventV1),
        Summary = "Published if an order is cancelled.")]
    public async Task PublishOrderCancelledEventV1(OrderCancelledEventV1 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();

        using var activity = Activity.Current?.Source.StartActivityWithSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            eventType,
            eventId,
            "dapr",
            PubSubName,
            configuration[_clientId] ?? "",
            evt.OrderIdentifier
        ));

        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, Source },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType },
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DateFormat) }
        };
        await CacheOrder(evt.OrderIdentifier);
        await daprClient.PublishEventAsync(PubSubName, eventType, evt, eventMetadata);
    }

    public async Task PublishOrderCreatedEventV2(OrderCreatedEventV2 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();

        using var activity = Activity.Current?.Source.StartActivityWithSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            eventType,
            eventId,
            "dapr",
            PubSubName,
            configuration[_clientId] ?? "",
            evt.OrderId
        ));

        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, Source },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType },
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DateFormat) }
        };
        await CacheOrder(evt.OrderId);
        await daprClient.PublishEventAsync(PubSubName, eventType, evt, eventMetadata);
    }

    private async Task CacheOrder(string orderIdentifier)
    {
        var order = await repository.Retrieve(orderIdentifier);

        await cache.SetStringAsync(order.OrderIdentifier, JsonSerializer.Serialize(new OrderDto(order)));
    }
}