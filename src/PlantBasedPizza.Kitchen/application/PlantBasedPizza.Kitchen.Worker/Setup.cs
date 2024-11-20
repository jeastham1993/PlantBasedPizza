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
        var handler = app.Services.GetRequiredService<OrderSubmittedEventHandler>();

        app.MapPost("/order-submitted",
            [Topic("public", "order.orderSubmitted.v1")] async (
                OrderSubmittedEventV1 evt) =>
            {
                await handler.Handle(evt);
                
                return Results.Ok();
            });

        return app;
    }
}