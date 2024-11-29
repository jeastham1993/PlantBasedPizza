using System.Diagnostics;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.Payments.ExternalEvents;
using PlantBasedPizza.Payments.OrderSubmitted;

namespace PlantBasedPizza.Payments;

public static class EventHandlers
{
    public static WebApplication AddEventHandlers(this WebApplication app)
    {
        app.MapPost("/order-submitted",
            [Topic("public", "order.orderSubmitted.v1")]
            async ([FromServices] OrderSubmittedEventHandler handler, IDistributedCache cache, HttpContext ctx,
                OrderSubmittedEventV1 evt) =>
            {
                try
                {
                    var cloudEventId = ctx.ExtractEventId();
                
                    var cachedEvent = await cache.GetStringAsync($"events_{cloudEventId}");

                    if (cachedEvent != null)
                    {
                        Activity.Current?.AddTag("events.idempotent", "true");
                        return Results.Ok();
                    }
                
                    var result = await handler.Handle(evt);

                    if (!result)
                    {
                        return Results.InternalServerError();
                    }

                    await cache.SetStringAsync($"events_{cloudEventId}", "processed", new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    });

                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Activity.Current?.AddException(ex);
                    
                    return Results.InternalServerError();
                }
            });

        return app;
    }
}