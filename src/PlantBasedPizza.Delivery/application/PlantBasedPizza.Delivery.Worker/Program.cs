using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Delivery.Worker;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddEnvironmentVariables();

var serviceName = "DeliveryWorker";

builder.Services
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddMessaging(builder.Configuration)
    .AddDeliveryInfrastructure(builder.Configuration);

builder.Services.AddSingleton<OrderReadyForDeliveryEventHandler>();

builder.Services.AddHostedService<OrderReadyForDeliveryEventWorker>();

var app = builder.Build();

app.MapGet("/deliver/health", () => "Healthy");

app.Run();