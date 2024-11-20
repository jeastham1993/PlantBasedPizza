using Dapr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Deliver.Core.IntegrationEvents;

namespace PlantBasedPizza.Delivery.Worker;

public static class Setup
{
    public static WebApplication AddReadyForDeliveryHandler(this WebApplication app)
    {
        var handler = app.Services.GetRequiredService<OrderReadyForDeliveryEventHandler>();

        app.MapPost("/ready-for-delivery",
            [Topic("public", "order.readyForDelivery.v1")] async (
                OrderReadyForDeliveryEventV1 evt) =>
            {
                await handler.Handle(evt);
                
                return Results.Ok();
            });

        return app;
    }
}