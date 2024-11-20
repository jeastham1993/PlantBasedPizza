using Dapr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Orders.Worker.Handlers;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker;

public static class Setup
{
    public static WebApplication AddEventHandlers(this WebApplication app)
    {
        var driverCollectedHandler = app.Services.GetRequiredService<DriverCollectedOrderEventHandler>();
        app.MapPost("/driver-collected",
            [Topic("public", "delivery.driverCollectedOrder.v1")]
            async (
                DriverCollectedOrderEventV1 evt) =>
            {
                await driverCollectedHandler.Handle(evt);

                return Results.Ok();
            });

        var driverDeliveredHandler = app.Services.GetRequiredService<DriverDeliveredOrderEventHandler>();
        app.MapPost("/driver-delivered",
            [Topic("public", "delivery.driverDeliveredOrder.v1")]
            async (
                DriverDeliveredOrderEventV1 evt) =>
            {
                await driverDeliveredHandler.Handle(evt);

                return Results.Ok();
            });

        var cache = app.Services.GetRequiredService<IDistributedCache>();
        app.MapPost("/loyalty-updated",
            [Topic("public", "loyalty.customerLoyaltyPointsUpdated.v1")]
            async (
                CustomerLoyaltyPointsUpdatedEvent evt) =>
            {
                await cache.SetStringAsync(evt.CustomerIdentifier.ToUpper(),
                    evt.TotalLoyaltyPoints.ToString("n0"));

                return Results.Ok();
            });

        var orderBakedHandler = app.Services.GetRequiredService<OrderBakedEventHandler>();
        app.MapPost("/order-baked",
            [Topic("public", "kitchen.orderBaked.v1")]
            async (
                OrderBakedEventV1 evt) =>
            {
                await orderBakedHandler.Handle(evt);

                return Results.Ok();
            });

        var orderPrepHandler = app.Services.GetRequiredService<OrderPreparingEventHandler>();
        app.MapPost("/order-preparing",
            [Topic("public", "kitchen.orderPreparing.v1")]
            async (
                OrderPreparingEventV1 evt) =>
            {
                await orderPrepHandler.Handle(evt);

                return Results.Ok();
            });

        var orderPrepCompleteHandler = app.Services.GetRequiredService<OrderPrepCompleteEventHandler>();
        app.MapPost("/order-prep-complete",
            [Topic("public", "kitchen.orderPrepComplete.v1")]
            async (
                OrderPrepCompleteEventV1 evt) =>
            {
                await orderPrepCompleteHandler.Handle(evt);

                return Results.Ok();
            });

        var orderQualityCheckedHandler = app.Services.GetRequiredService<OrderQualityCheckedEventHandler>();
        app.MapPost("/order-quality-checked",
            [Topic("public", "kitchen.qualityChecked.v1")]
            async (
                OrderQualityCheckedEventV1 evt) =>
            {
                await orderQualityCheckedHandler.Handle(evt);

                return Results.Ok();
            });

        return app;
    }
}