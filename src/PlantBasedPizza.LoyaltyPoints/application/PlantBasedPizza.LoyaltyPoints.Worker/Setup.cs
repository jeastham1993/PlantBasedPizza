using Dapr;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

namespace PlantBasedPizza.LoyaltyPoints.Worker;

public static class Setup
{
    public static WebApplication AddLoyaltyPointsEventHandler(this WebApplication app)
    {
        var handler = app.Services.GetRequiredService<AddLoyaltyPointsCommandHandler>();

        app.MapPost("/order-completed-event",
            [Topic("public", "order.orderCompleted.v1")]
            async (
                OrderCompletedEvent evt) =>
            {
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