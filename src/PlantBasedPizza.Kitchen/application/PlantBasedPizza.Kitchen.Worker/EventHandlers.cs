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
    private const string OrderConfirmedEventName = "internal.kitchen.orderConfirmed.v1";
    private const string FailedMessagesEventName = "kitchen.failedMessages";
    
    [Topic("kitchen", OrderConfirmedEventName,
        DeadLetterTopic = "kitchen.failedMessages")]
    public static async Task<IResult> HandleOrderConfirmedEvent([FromServices] OrderConfirmedEventHandler handler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        OrderConfirmed evt)
    {
        try
        {
            var eventId = httpContext.ExtractEventId();
        
            using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(new SemanticConventions(
                EventType.PUBLIC,
                OrderConfirmedEventName,
                eventId,
                "dapr",
                "kitchen",
                configuration["ApplicationConfig:ApplicationName"] ?? "",
                evt.OrderIdentifier
            ));

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

    [Topic("kitchen", FailedMessagesEventName)]
    public static async Task<IResult> HandleDeadLetterMessage(
        [FromServices] IDeadLetterRepository deadLetterRepository,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        object data)
    {
        var eventData = httpContext.ExtractEventData();
        
        using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            FailedMessagesEventName,
            eventData.EventId,
            "dapr",
            "kitchen",
            configuration["ApplicationConfig:ApplicationName"] ?? ""
        ));

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