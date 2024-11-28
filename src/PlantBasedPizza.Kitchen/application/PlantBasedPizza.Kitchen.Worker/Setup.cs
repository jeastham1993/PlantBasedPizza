using Dapr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PlantBasedPizza.Kitchen.Worker.Handlers;
using PlantBasedPizza.Kitchen.Worker.IntegrationEvents;

namespace PlantBasedPizza.Kitchen.Worker;

public static class Setup
{
    public static WebApplication AddOrderSubmittedProcessing(this WebApplication app)
    {
        var handler = app.Services.GetRequiredService<OrderConfirmedEventHandler>();

        app.MapPost("/order-confirmed",
            [Topic("public", "order.orderConfirmed.v1")] async (
                OrderConfirmedEventV1 evt) =>
            {
                await handler.Handle(evt);
                
                return Results.Ok();
            });

        return app;
    }
}