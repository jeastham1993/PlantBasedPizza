using System.Diagnostics;
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
    private const string ReadyForDeliveryEventName = "order.readyForDelivery.v1";
    private const string FailedMessagesEventName = "delivery.failedMessages";
    
    [Topic("public", ReadyForDeliveryEventName,
        DeadLetterTopic = "delivery.failedMessages")]
    public static async Task<IResult> HandleOrderReadyForDeliveryEvent([FromServices]OrderReadyForDeliveryEventHandler handler,
        [FromServices] Idempotency idempotency,
        [FromServices] IConfiguration configuration,
        HttpContext httpContext,
        OrderReadyForDeliveryEventV1 evt)
    {
        var eventId = httpContext.ExtractEventId();
        
        using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            ReadyForDeliveryEventName,
            eventId,
            "dapr",
            "public",
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

    [Topic("public", FailedMessagesEventName)]
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
            "public",
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