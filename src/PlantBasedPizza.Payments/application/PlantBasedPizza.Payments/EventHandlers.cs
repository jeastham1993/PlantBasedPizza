using Dapr;
using PlantBasedPizza.Payments.ExternalEvents;

namespace PlantBasedPizza.Payments;

public static class EventHandlers
{
    public static WebApplication AddEventHandlers(this WebApplication app)
    {
        var orderSubmittedPrivateEventHandler = app.Services.GetRequiredService<OrderSubmittedEventHandler>();
        app.MapPost("/order-submitted",
            [Topic("public", "order.orderSubmitted.v1")]
            async (
                OrderSubmittedEventV1 evt) =>
            {
                await orderSubmittedPrivateEventHandler.Handle(evt);

                return Results.Ok();
            });

        return app;
    }
}