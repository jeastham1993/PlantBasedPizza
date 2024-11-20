using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Delivery.Worker;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddEnvironmentVariables();

builder.Services.AddDaprClient();

var serviceName = "DeliveryWorker";

builder.Services
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddMessaging(builder.Configuration)
    .AddDeliveryInfrastructure(builder.Configuration);

builder.Services.AddSingleton<OrderReadyForDeliveryEventHandler>();

var app = builder.Build();

app.MapSubscribeHandler();
app.UseCloudEvents();
app.AddReadyForDeliveryHandler();

app.MapGet("/deliver/health", () => "Healthy");

app.Run();