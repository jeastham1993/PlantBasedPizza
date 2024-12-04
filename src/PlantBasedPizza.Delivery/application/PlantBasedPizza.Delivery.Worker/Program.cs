using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Deliver.Core.OrderReadyForDelivery;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Delivery.Worker;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddEnvironmentVariables();
builder.AddLoggerConfigs();

builder.Services.AddDaprClient();

builder.Services
    .AddSharedInfrastructure(builder.Configuration, ApplicationDefaults.ServiceName)
    .AddDeliveryInfrastructure(builder.Configuration)
    .AddHostedService<OutboxWorker>();

builder.Services.AddSingleton<OrderReadyForDeliveryEventHandler>();
builder.Services.AddSingleton<Idempotency, CachedIdempotencyService>();

var app = builder.Build();

app.MapSubscribeHandler();
app.UseCloudEvents();

app.MapPost("/ready-for-delivery",  EventHandlers.HandleOrderReadyForDeliveryEvent);
app.MapPost("/errors", EventHandlers.HandleDeadLetterMessage);

app.MapGet("/deliver/health", () => "Healthy");

await app.RunAsync();