using System.Diagnostics;
using Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Worker.Handlers;
using PlantBasedPizza.Kitchen.Worker.IntegrationEvents;

namespace PlantBasedPizza.Kitchen.Worker;

public static class EventHandlers
{
    [Topic("public", "order.orderConfirmed.v1")]
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
}