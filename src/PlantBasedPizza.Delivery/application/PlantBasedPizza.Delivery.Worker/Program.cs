using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Delivery.Worker;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddEnvironmentVariables();
builder.AddLoggerConfigs();

builder.Services.AddDaprClient();

var serviceName = "DeliveryWorker";

builder.Services
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddDeliveryInfrastructure(builder.Configuration);

builder.Services.AddSingleton<OrderReadyForDeliveryEventHandler>();

var app = builder.Build();

app.MapSubscribeHandler();
app.UseCloudEvents();
app.AddReadyForDeliveryHandler();

app.MapGet("/deliver/health", () => "Healthy");

app.Run();