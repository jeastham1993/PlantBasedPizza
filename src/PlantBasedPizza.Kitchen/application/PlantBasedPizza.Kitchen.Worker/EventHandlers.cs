using System.Diagnostics;
using System.Text.Json;
using Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.OrderConfirmed;
using PlantBasedPizza.Kitchen.Infrastructure;

namespace PlantBasedPizza.Kitchen.Worker;

public static class EventHandlers
{
    [Topic("public", "order.orderConfirmed.v1",
        DeadLetterTopic = "kitchen.failedMessages")]
    public static async Task<IResult> HandleOrderConfirmedEvent([FromServices] OrderConfirmedEventHandler handler,
        [FromServices] Idempotency idempotency,
        HttpContext httpContext,
        OrderConfirmedEventV1 evt)
    {
        try
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
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            
            return Results.InternalServerError();
        }
    }

    [Topic("public", "kitchen.failedMessages")]
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