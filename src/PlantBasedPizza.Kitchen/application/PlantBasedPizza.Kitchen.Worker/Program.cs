using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Kitchen.Worker;
using PlantBasedPizza.Kitchen.Worker.Handlers;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddEnvironmentVariables();

var serviceName = "KitchenWorker";

builder.Services
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddMessaging(builder.Configuration)
    .AddKitchenInfrastructure(builder.Configuration);

builder.Services.AddSingleton<OrderSubmittedEventHandler>();

builder.Services.AddHostedService<OrderSubmittedEventWorker>();

var app = builder.Build();

app.MapGet("/kitchen/health", () => "Healthy");

app.Run();