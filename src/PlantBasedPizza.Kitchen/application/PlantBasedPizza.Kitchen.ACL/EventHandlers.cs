using System.Diagnostics;
using System.Text.Json;
using Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.ACL.Events;
using PlantBasedPizza.Kitchen.Core.OrderConfirmed;
using PlantBasedPizza.Kitchen.Infrastructure;

namespace PlantBasedPizza.Kitchen.ACL;

public static class EventHandlers
{
    private const string OrderConfirmedEventName = "order.orderConfirmed.v1";
    private const string FailedMessagesEventName = "kitchen.failedMessages";

    [Topic("public", OrderConfirmedEventName,
        DeadLetterTopic = "kitchen.failedMessages")]
    public static async Task<IResult> HandleOrderConfirmedEvent([FromServices] OrderConfirmedEventHandler handler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        [FromServices] EventAdapter adapter,
        HttpContext httpContext,
        OrderConfirmedEventV1 evt)
    {
        try
        {
            var eventData = httpContext.ExtractEventData();

            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
                new SemanticConventions(
                    EventType.PUBLIC,
                    OrderConfirmedEventName,
                    eventData.EventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    evt.OrderIdentifier
                ), new List<ActivityLink>(1)
                {
                    new(ActivityContext.Parse(eventData.TraceParent, null))
                });

            if (await idempotency.HasEventBeenProcessedWithId(eventData.EventId)) return Results.Ok();

            await adapter.Translate(evt);

            await idempotency.ProcessedSuccessfully(eventData.EventId);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);

            return Results.InternalServerError();
        }
    }

    [Topic("public", FailedMessagesEventName)]
    public static async Task<IResult> HandleDeadLetterMessage(
        [FromServices] IDeadLetterRepository deadLetterRepository,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        object data)
    {
        var eventData = httpContext.ExtractEventData();

        using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(
            new SemanticConventions(
                EventType.PUBLIC,
                FailedMessagesEventName,
                eventData.EventId,
                "dapr",
                "public",
                configuration["ApplicationConfig:ApplicationName"] ?? ""
            ), new List<ActivityLink>(1)
            {
                new(ActivityContext.Parse(eventData.TraceParent, null))
            });

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