using System.Diagnostics;
using System.Text.Json;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

namespace PlantBasedPizza.LoyaltyPoints.Worker;

public static class EventHandlers
{
    private const string OrderCompletedEventName = "order.orderCompleted.v1";
    private const string FailedMessagesEventName = "loyalty.failedMessages";
    
    [Topic("public", OrderCompletedEventName,
        DeadLetterTopic = FailedMessagesEventName)]
    public static async Task<IResult> HandleOrderCompletedEvent([FromServices] AddLoyaltyPointsCommandHandler handler,
        [FromServices] IConfiguration configuration,
        HttpContext context,
        OrderCompletedEvent evt)
    {
        var eventData = context.ExtractEventData();
        
        using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(new SemanticConventions(
            EventType.PUBLIC,
            OrderCompletedEventName,
            eventData.EventId,
            "dapr",
            "public",
            configuration["ApplicationConfig:ApplicationName"] ?? "",
            evt.OrderIdentifier
        ), new List<ActivityLink>(1)
        {
            new(ActivityContext.Parse(eventData.TraceParent, null))
        });
                
        await handler.Handle(new AddLoyaltyPointsCommand
        {
            CustomerIdentifier = evt.CustomerIdentifier,
            OrderValue = evt.OrderValue,
            OrderIdentifier = evt.OrderIdentifier
        });

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