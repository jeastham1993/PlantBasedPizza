using System.Diagnostics;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.Orders.Worker.Handlers;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker;

public static class EventHandlers
{
    // You've still got an event handler for that payments.paymentSuccessful event!
    [Topic("public", "payments.paymentSuccessful.v1")]
    public static async Task<IResult> HandlePaymentSuccessfulEvent(
        [FromServices] PaymentSuccessEventHandler paymentSuccessEventHandler,
        [FromServices] Idempotency idempotency,
        HttpContext httpContext,
        PaymentSuccessfulEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

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
    
    [Topic("public", "delivery.driverCollectedOrder.v1")]
    public static async Task<IResult> HandleDriverCollectedOrderEvent(
        [FromServices]DriverCollectedOrderEventHandler driverCollectedOrderEventHandler,
        [FromServices] Idempotency idempotency,
        HttpContext httpContext,
        DriverCollectedOrderEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

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
    
    [Topic("public", "delivery.driverDeliveredOrder.v1")]
    public static async Task<IResult> HandleDriverDeliveredOrderEvent(
        [FromServices]DriverDeliveredOrderEventHandler driverDeliveredOrderEventHandler,
        [FromServices] Idempotency idempotency,
        HttpContext httpContext,
        DriverDeliveredOrderEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

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
    
    [Topic("public", "loyalty.customerLoyaltyPointsUpdated.v1")]
    public static async Task<IResult> HandleLoyaltyPointsUpdatedEvent(
        [FromServices] IDistributedCache cache,
        [FromServices] Idempotency idempotency,
        HttpContext httpContext,
        CustomerLoyaltyPointsUpdatedEvent evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

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
    
    [Topic("public", "kitchen.orderBaked.v1")]
    public static async Task<IResult> HandleOrderBakedEvent(
        [FromServices] OrderBakedEventHandler orderBakedHandler,
        [FromServices] Idempotency idempotency,
        HttpContext httpContext,
        OrderBakedEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

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
    
    [Topic("public", "kitchen.orderPreparing.v1")]
    public static async Task<IResult> HandleOrderPreparingEvent(
        [FromServices] OrderPreparingEventHandler orderPreparingEventHandler,
        [FromServices] Idempotency idempotency,
        HttpContext httpContext,
        OrderPreparingEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

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
    
    [Topic("public", "kitchen.orderPrepComplete.v1")]
    public static async Task<IResult> HandleOrderPrepCompleteEvent(
        [FromServices] OrderPrepCompleteEventHandler orderPrepCompleteHandler,
        [FromServices] Idempotency idempotency,
        HttpContext httpContext,
        OrderPrepCompleteEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

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
    
    [Topic("public", "kitchen.qualityChecked.v1")]
    public static async Task<IResult> HandleOrderQualityCheckedEvent(
        [FromServices] OrderQualityCheckedEventHandler orderQualityCheckedEventHandler,
        [FromServices] Idempotency idempotency,
        HttpContext httpContext,
        OrderQualityCheckedEventV1 evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();

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
}