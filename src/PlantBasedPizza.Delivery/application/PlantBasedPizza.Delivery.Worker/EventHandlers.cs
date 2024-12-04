using System.Text.Json;
using Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Deliver.Core.OrderReadyForDelivery;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Delivery.Worker;

public static class EventHandlers
{
    [Topic("public", "order.readyForDelivery.v1",
        DeadLetterTopic = "delivery.failedMessages")]
    public static async Task<IResult> HandleOrderReadyForDeliveryEvent([FromServices]OrderReadyForDeliveryEventHandler handler,
        [FromServices] Idempotency idempotency,
        HttpContext httpContext,
        OrderReadyForDeliveryEventV1 evt)
    {
        var eventId = httpContext.ExtractEventId();
                
        if (await idempotency.HasEventBeenProcessedWithId(eventId))
        {
            return Results.Ok();
        }
                
        await handler.Handle(evt);
                
        await idempotency.ProcessedSuccessfully(eventId);
                
        return Results.Ok();
    }

    [Topic("public", "delivery.failedMessages")]
    public static async Task<IResult> HandleDeadLetterMessage(
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