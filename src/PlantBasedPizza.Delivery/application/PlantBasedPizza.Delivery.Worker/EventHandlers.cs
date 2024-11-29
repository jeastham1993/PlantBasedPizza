using Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Deliver.Core.OrderReadyForDelivery;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Delivery.Worker;

public static class EventHandlers
{
    [Topic("public", "order.readyForDelivery.v1")]
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
}