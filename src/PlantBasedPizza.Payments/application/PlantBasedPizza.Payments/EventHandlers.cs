using System.Diagnostics;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.Payments.RefundPayment;
using PlantBasedPizza.Payments.TakePayment;

namespace PlantBasedPizza.Payments;

public static class EventHandlers
{
    private const string TakePaymentCommandName = "payments.takepayment.v1";
    private const string RefundPaymentCommandName = "payments.refundpayment.v1";
    
    public static WebApplication AddEventHandlers(this WebApplication app)
    {
        app.MapPost("/take-payment",
            [Topic("payments", TakePaymentCommandName)]
            async ([FromServices] TakePaymentCommandHandler handler, [FromServices] IConfiguration configuration, IDistributedCache cache, HttpContext ctx,
                TakePaymentCommand command) =>
            {
                try
                {
                    var eventId = ctx.ExtractEventId();

                    using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                        new SemanticConventions(
                            EventType.PUBLIC,
                            TakePaymentCommandName,
                            eventId ?? "",
                            "dapr",
                            "public",
                            configuration["ApplicationConfig:ApplicationName"] ?? "",
                            command.OrderIdentifier
                        ));
                
                    var cachedEvent = await cache.GetStringAsync($"events_{eventId}");
                    
                    processActivity?.AddTag("orderIdentifier", command.OrderIdentifier ?? "null");
                    processActivity?.AddTag("paymentAmount", command.PaymentAmount.ToString("n2"));

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

                    await cache.SetStringAsync($"events_{eventId}", "processed", new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    });

                    return Results.Ok();
                }
                catch (ArgumentNullException ex)
                {
                    Activity.Current?.AddException(ex);
                    
                    return Results.BadRequest();
                }
                catch (Exception ex)
                {
                    Activity.Current?.AddException(ex);

                    return Results.InternalServerError();
                }
            });
        
        app.MapPost("/refund-payment",
            [Topic("payments", RefundPaymentCommandName)]
            async ([FromServices] RefundPaymentCommandHandler handler, [FromServices] IConfiguration configuration, IDistributedCache cache, HttpContext ctx,
                RefundPaymentCommand command) =>
            {
                try
                {
                    var eventId = ctx.ExtractEventId();

                    using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                        new SemanticConventions(
                            EventType.PUBLIC,
                            RefundPaymentCommandName,
                            eventId,
                            "dapr",
                            "public",
                            configuration["ApplicationConfig:ApplicationName"] ?? "",
                            command.OrderIdentifier
                        ));
                
                    var cachedEvent = await cache.GetStringAsync($"events_{eventId}");

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

                    await cache.SetStringAsync($"events_{eventId}", "processed", new DistributedCacheEntryOptions()
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