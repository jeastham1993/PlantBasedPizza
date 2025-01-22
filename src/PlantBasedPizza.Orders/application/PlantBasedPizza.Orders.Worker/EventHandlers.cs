using System.Diagnostics;
using System.Text.Json;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.DriverCollectedOrder;
using PlantBasedPizza.OrderManager.Core.DriverDeliveredOrder;
using PlantBasedPizza.OrderManager.Core.KitchenConfirmedOrder;
using PlantBasedPizza.OrderManager.Core.OrderBaked;
using PlantBasedPizza.OrderManager.Core.OrderPreparing;
using PlantBasedPizza.OrderManager.Core.OrderPrepComplete;
using PlantBasedPizza.OrderManager.Core.OrderQualityChecked;
using PlantBasedPizza.OrderManager.Core.PaymentSuccess;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker;

public static class EventHandlers
{
    private const string PaymentSuccessfulEventName = "payments.paymentSuccessful.v1";
    private const string DriverCollectedOrderEventName = "delivery.driverCollectedOrder.v1";
    private const string DriverDeliveredOrderEventName = "delivery.driverDeliveredOrder.v1";
    private const string LoyaltyPointsUpdatedEventName = "loyalty.customerLoyaltyPointsUpdated.v1";
    private const string KitchenOrderConfirmedEventName = "kitchen.orderConfirmed.v1";
    private const string OrderBakedEventName = "kitchen.orderBaked.v1";
    private const string OrderPreparingEventName = "kitchen.orderPreparing.v1";
    private const string OrderPrepCompleteEventName = "kitchen.orderPrepComplete.v1";
    private const string OrderQualityCheckedEventName = "kitchen.qualityChecked.v1";
    private const string FailedMessagesEventName = "orders.failedMessages";

    [Topic("public",
        PaymentSuccessfulEventName,
        DeadLetterTopic = FailedMessagesEventName)]
    public static async Task<IResult> HandlePaymentSuccessfulEvent(
        [FromServices] PaymentSuccessEventHandler paymentSuccessEventHandler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        PaymentSuccessfulEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    PaymentSuccessfulEventName,
                    eventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    evt.OrderIdentifier
                ));

            if (await idempotency.HasEventBeenProcessedWithId(eventId)) return Results.Ok();

            await paymentSuccessEventHandler.Handle(evt);

            await idempotency.ProcessedSuccessfully(eventId);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            Activity.Current?.AddTag("messaging.error", true);

            return Results.InternalServerError();
        }
    }

    [Topic("public", DriverCollectedOrderEventName,
        DeadLetterTopic = FailedMessagesEventName)]
    public static async Task<IResult> HandleDriverCollectedOrderEvent(
        [FromServices] DriverCollectedOrderEventHandler driverCollectedOrderEventHandler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        DriverCollectedOrderEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    DriverCollectedOrderEventName,
                    eventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    evt.OrderIdentifier
                ));

            if (await idempotency.HasEventBeenProcessedWithId(eventId)) return Results.Ok();

            await driverCollectedOrderEventHandler.Handle(evt);

            await idempotency.ProcessedSuccessfully(eventId);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            Activity.Current?.AddTag("messaging.error", true);

            return Results.InternalServerError();
        }
    }

    [Topic("public", DriverDeliveredOrderEventName,
        DeadLetterTopic = FailedMessagesEventName)]
    public static async Task<IResult> HandleDriverDeliveredOrderEvent(
        [FromServices] DriverDeliveredOrderEventHandler driverDeliveredOrderEventHandler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        DriverDeliveredOrderEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    DriverDeliveredOrderEventName,
                    eventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    evt.OrderIdentifier
                ));

            if (await idempotency.HasEventBeenProcessedWithId(eventId)) return Results.Ok();

            await driverDeliveredOrderEventHandler.Handle(evt);

            await idempotency.ProcessedSuccessfully(eventId);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            Activity.Current?.AddTag("messaging.error", true);

            return Results.InternalServerError();
        }
    }

    [Topic("public", LoyaltyPointsUpdatedEventName,
        DeadLetterTopic = FailedMessagesEventName)]
    public static async Task<IResult> HandleLoyaltyPointsUpdatedEvent(
        [FromServices] IDistributedCache cache,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        CustomerLoyaltyPointsUpdatedEvent evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    LoyaltyPointsUpdatedEventName,
                    eventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? ""
                ));

            if (await idempotency.HasEventBeenProcessedWithId(eventId)) return Results.Ok();

            await cache.SetStringAsync(evt.CustomerIdentifier.ToUpper(),
                evt.TotalLoyaltyPoints.ToString("n0"));

            await idempotency.ProcessedSuccessfully(eventId);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            Activity.Current?.AddTag("messaging.error", true);

            return Results.InternalServerError();
        }
    }
    
    [Topic("public", KitchenOrderConfirmedEventName,
        DeadLetterTopic = FailedMessagesEventName)]
    public static async Task<IResult> HandleKitchenOrderConfirmedEvent(
        [FromServices] KitchenConfirmedOrderEventHandler kitchenConfirmedEventHandler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        KitchenConfirmedOrderEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    OrderBakedEventName,
                    eventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    evt.OrderIdentifier
                ));

            if (await idempotency.HasEventBeenProcessedWithId(eventId)) return Results.Ok();

            await kitchenConfirmedEventHandler.Handle(evt);

            await idempotency.ProcessedSuccessfully(eventId);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            Activity.Current?.AddTag("messaging.error", true);

            return Results.InternalServerError();
        }
    }

    [Topic("public", OrderBakedEventName,
        DeadLetterTopic = FailedMessagesEventName)]
    public static async Task<IResult> HandleOrderBakedEvent(
        [FromServices] OrderBakedEventHandler orderBakedHandler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        OrderBakedEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    OrderBakedEventName,
                    eventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    evt.OrderIdentifier
                ));

            if (await idempotency.HasEventBeenProcessedWithId(eventId)) return Results.Ok();

            await orderBakedHandler.Handle(evt);

            await idempotency.ProcessedSuccessfully(eventId);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            Activity.Current?.AddTag("messaging.error", true);

            return Results.InternalServerError();
        }
    }

    [Topic("public", OrderPreparingEventName,
        DeadLetterTopic = FailedMessagesEventName)]
    public static async Task<IResult> HandleOrderPreparingEvent(
        [FromServices] OrderPreparingEventHandler orderPreparingEventHandler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        OrderPreparingEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    OrderPreparingEventName,
                    eventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    evt.OrderIdentifier
                ));

            if (await idempotency.HasEventBeenProcessedWithId(eventId)) return Results.Ok();

            await orderPreparingEventHandler.Handle(evt);

            await idempotency.ProcessedSuccessfully(eventId);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            Activity.Current?.AddTag("messaging.error", true);

            return Results.InternalServerError();
        }
    }

    [Topic("public", OrderPrepCompleteEventName,
        DeadLetterTopic = FailedMessagesEventName)]
    public static async Task<IResult> HandleOrderPrepCompleteEvent(
        [FromServices] OrderPrepCompleteEventHandler orderPrepCompleteHandler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        OrderPrepCompleteEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    OrderPrepCompleteEventName,
                    eventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    evt.OrderIdentifier
                ));

            if (await idempotency.HasEventBeenProcessedWithId(eventId)) return Results.Ok();

            await orderPrepCompleteHandler.Handle(evt);

            await idempotency.ProcessedSuccessfully(eventId);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            Activity.Current?.AddTag("messaging.error", true);

            return Results.InternalServerError();
        }
    }

    [Topic("public", OrderQualityCheckedEventName,
        DeadLetterTopic = FailedMessagesEventName)]
    public static async Task<IResult> HandleOrderQualityCheckedEvent(
        [FromServices] OrderQualityCheckedEventHandler orderQualityCheckedEventHandler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        OrderQualityCheckedEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    OrderQualityCheckedEventName,
                    eventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    evt.OrderIdentifier
                ));

            if (await idempotency.HasEventBeenProcessedWithId(eventId)) return Results.Ok();

            await orderQualityCheckedEventHandler.Handle(evt);

            await idempotency.ProcessedSuccessfully(eventId);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            Activity.Current?.AddTag("messaging.error", true);

            return Results.InternalServerError();
        }
    }

    [Topic("public", FailedMessagesEventName)]
    public static async Task<IResult> HandleDeadLetterMessage(
        [FromServices] ILogger<PaymentSuccessfulEventV1> logger,
        [FromServices] IDeadLetterRepository deadLetterRepository,
        HttpContext httpContext,
        object data)
    {
        var eventData = httpContext.ExtractEventData();

        await deadLetterRepository.StoreAsync(new DeadLetterMessage
        {
            EventId = eventData.EventId,
            EventType = eventData.EventType,
            EventData = JsonSerializer.Serialize(data),
            TraceParent = eventData.TraceParent
        });

        return Results.Ok();
    }
}