using System.Diagnostics;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

namespace PlantBasedPizza.LoyaltyPoints.Worker;

public static class Setup
{
    private const string OrderCompletedEventName = "order.orderCompleted.v1";
    
    public static WebApplication AddLoyaltyPointsEventHandler(this WebApplication app)
    {
        app.MapPost("/order-completed-event",
            [Topic("public", OrderCompletedEventName)]
            async (
                [FromServices] AddLoyaltyPointsCommandHandler handler,
                [FromServices] IConfiguration configuration,
                HttpContext context,
                OrderCompletedEvent evt) =>
            {
                var eventId = context.ExtractEventId();
        
                using var processActivity = Activity.Current?.Source.StartActivityWithProcessSemanticConventions(new SemanticConventions(
                    EventType.PUBLIC,
                    OrderCompletedEventName,
                    eventId,
                    "dapr",
                    "public",
                    configuration["ApplicationConfig:ApplicationName"] ?? "",
                    evt.OrderIdentifier
                ));
                
                await handler.Handle(new AddLoyaltyPointsCommand
                {
                    CustomerIdentifier = evt.CustomerIdentifier,
                    OrderValue = evt.OrderValue,
                    OrderIdentifier = evt.OrderIdentifier
                });

                return Results.Ok();
            });

        return app;
    }
}