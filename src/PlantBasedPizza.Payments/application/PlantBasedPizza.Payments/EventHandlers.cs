using System.Diagnostics;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.Payments.RefundPayment;
using PlantBasedPizza.Payments.TakePayment;

namespace PlantBasedPizza.Payments;

public class EventHandlerLogger { }

public static class EventHandlers
{
    public static WebApplication AddEventHandlers(this WebApplication app)
    {
        app.MapPost("/take-payment",
            [Topic("payments", "payments.takepayment.v1")]
            async ([FromServices] TakePaymentCommandHandler handler, ILogger<EventHandlerLogger> logger, IDistributedCache cache, HttpContext ctx,
                TakePaymentCommand command) =>
            {
                try
                {
                    Activity.Current?.AddTag("orderIdentifier", command.OrderIdentifier ?? "null");
                    Activity.Current?.AddTag("paymentAmount", command.PaymentAmount.ToString("n2"));

                    logger.LogInformation("Received request to take payment");

                    var cloudEventId = ctx.ExtractEventId();

                    var cachedEvent = await cache.GetStringAsync($"events_{cloudEventId}");

                    if (cachedEvent != null)
                    {
                        Activity.Current?.AddTag("events.idempotent", "true");
                        return Results.Ok();
                    }

                    var result = await handler.Handle(command);

                    if (!result)
                    {
                        return Results.BadRequest();
                    }

                    await cache.SetStringAsync($"events_{cloudEventId}", "processed", new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    });

                    return Results.Ok();
                }
                catch (ArgumentNullException ex)
                {
                    logger.LogError(ex, "Failure taking payment");
                    Activity.Current?.AddException(ex);
                    
                    return Results.BadRequest();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failure taking payment");
                    Activity.Current?.AddException(ex);

                    return Results.InternalServerError();
                }
            });
        
        app.MapPost("/refund-payment",
            [Topic("payments", "payments.refundpayment.v1")]
            async ([FromServices] RefundPaymentCommandHandler handler, IDistributedCache cache, HttpContext ctx,
                RefundPaymentCommand command) =>
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
                
                    var result = await handler.Handle(command);

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