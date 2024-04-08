using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Events;
using PlantBasedPizza.Orders.Worker;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);

var serviceName = "OrdersWorker";

builder.Services
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddMessaging(builder.Configuration);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["RedisConnectionString"];
    options.InstanceName = "Orders";
});

builder.Services.AddHostedService<LoyaltyPointsUpdatedCacheWorker>();

var app = builder.Build();

app.MapGet("/orders/health", () => "Healthy");

app.Run();