using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Orders.Worker;
using PlantBasedPizza.Orders.Worker.Handlers;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddSharedInfrastructure(builder.Configuration, builder.Configuration["SERVICE_NAME"])
    .AddMessaging(builder.Configuration)
    .AddOrderManagerInfrastructure(builder.Configuration);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["RedisConnectionString"];
    options.InstanceName = "Orders";
});

builder.Services.Configure<QueueConfiguration>(builder.Configuration.GetSection("QueueConfiguration"));

builder.Services.AddSingleton<DriverCollectedOrderEventHandler>();
builder.Services.AddSingleton<DriverDeliveredOrderEventHandler>();
builder.Services.AddSingleton<OrderBakedEventHandler>();
builder.Services.AddSingleton<OrderPreparingEventHandler>();
builder.Services.AddSingleton<OrderPrepCompleteEventHandler>();
builder.Services.AddSingleton<OrderQualityCheckedEventHandler>();

// builder.Services.AddHostedService<DriverCollectedOrderEventWorker>();
// builder.Services.AddHostedService<DriverDeliveredOrderEventWorker>();
// builder.Services.AddHostedService<OrderBakedEventWorker>();
// builder.Services.AddHostedService<OrderPreparingEventWorker>();
// builder.Services.AddHostedService<OrderPrepCompleteEventWorker>();
builder.Services.AddHostedService<LoyaltyPointsUpdatedCacheWorker>();
builder.Services.AddHostedService<OrderQualityCheckedEventWorker>();

var app = builder.Build();

app.MapGet("/orders/health", () => "Healthy");

app.Run();