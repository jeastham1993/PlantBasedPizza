using Dapr;
using Google.Api;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

namespace PlantBasedPizza.LoyaltyPoints.Worker;

public static class Setup
{
    public static WebApplication AddLoyaltyPointsEventHandler(this WebApplication app)
    {
        
        app.MapPost("/order-completed-event",
            [Topic("public", "order.orderCompleted.v1")]
            async (
                [FromServices] AddLoyaltyPointsCommandHandler handler,
                HttpContext context,
                OrderCompletedEvent evt) =>
            {
                var eventId = context.ExtractEventId();
                
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