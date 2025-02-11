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

    [Topic("payments",
        TakePaymentCommandName)]
    public static async Task<IResult> HandleTakePaymentCommand([FromServices] TakePaymentCommandHandler handler,
        [FromServices] IConfiguration configuration,
        IDistributedCache cache,
        HttpContext context,
        TakePaymentCommand command)
    {
        try
        {
            var eventData = context.ExtractEventData();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    TakePaymentCommandName,
                    eventData.EventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    command.OrderIdentifier
                ), new List<ActivityLink>(1)
                {
                    new(ActivityContext.Parse(eventData.TraceParent, null))
                });

            var cachedEvent = await cache.GetStringAsync($"events_{eventData.EventId}");

            processActivity?.AddTag("orderIdentifier", command.OrderIdentifier ?? "null");
            processActivity?.AddTag("paymentAmount", command.PaymentAmount.ToString("n2"));

            if (cachedEvent != null)
            {
                Activity.Current?.AddTag("events.idempotent", "true");
                return Results.Ok();
            }

            var result = await handler.Handle(command);
            processActivity?.AddTag("paymentStatus", result.Status.ToString());

            // Only return a bad request if there is an unknown error, this will force Dapr to retry.
            if (result.Status == TakePaymentStatus.UNEXPECTED_ERROR)
            {
                return Results.BadRequest();
            }

            await cache.SetStringAsync($"events_{eventData.EventId}", "processed", new DistributedCacheEntryOptions()
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
    }


    [Topic("payments",
            RefundPaymentCommandName)]
    public static async Task<IResult> HandleRefundPaymentCommand([FromServices] RefundPaymentCommandHandler handler, [FromServices] IConfiguration configuration, IDistributedCache cache, HttpContext ctx,
                    HttpContext context,
                    RefundPaymentCommand command)
    {
        try
        {
            var eventData = ctx.ExtractEventData();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    RefundPaymentCommandName,
                    eventData.EventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    command.OrderIdentifier
                ), new List<ActivityLink>(1)
                {
                    new(ActivityContext.Parse(eventData.TraceParent, null))
                });

            var cachedEvent = await cache.GetStringAsync($"events_{eventData.EventId}");

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

            await cache.SetStringAsync($"events_{eventData.EventId}", "processed", new DistributedCacheEntryOptions()
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
    }
}