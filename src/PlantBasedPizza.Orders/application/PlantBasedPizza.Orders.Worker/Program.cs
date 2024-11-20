using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Orders.Worker;
using PlantBasedPizza.Orders.Worker.Handlers;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Caching;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddDaprClient();

var serviceName = "OrdersWorker";

builder.Services
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddMessaging(builder.Configuration)
    .AddOrderManagerInfrastructure(builder.Configuration);

builder.Services.AddCaching(builder.Configuration);

builder.Services.AddSingleton<DriverCollectedOrderEventHandler>();
builder.Services.AddSingleton<DriverDeliveredOrderEventHandler>();
builder.Services.AddSingleton<OrderBakedEventHandler>();
builder.Services.AddSingleton<OrderPreparingEventHandler>();
builder.Services.AddSingleton<OrderPrepCompleteEventHandler>();
builder.Services.AddSingleton<OrderQualityCheckedEventHandler>();

var app = builder.Build();

app.MapGet("/orders/health", () => "Healthy");

app.MapSubscribeHandler();
app.UseCloudEvents();
app.AddEventHandlers();

app.Run();