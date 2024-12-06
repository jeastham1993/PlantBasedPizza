using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Kitchen.Core.OrderConfirmed;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Kitchen.Worker;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Configuration
    .AddEnvironmentVariables();
builder.AddLoggerConfigs();

builder.Services.AddDaprClient();
builder.Services.AddSingleton<Idempotency, CachedIdempotencyService>();

builder.Services
    .AddSharedInfrastructure(builder.Configuration, ApplicationDefaults.ServiceName)
    .AddKitchenInfrastructure(builder.Configuration)
    .AddHostedService<OutboxWorker>();

builder.Services.AddSingleton<OrderConfirmedEventHandler>();

var app = builder.Build();

app.MapSubscribeHandler();
app.UseCloudEvents();

app.MapPost("/order-confirmed", EventHandlers.HandleOrderConfirmedEvent);
app.MapPost("/errors", EventHandlers.HandleDeadLetterMessage);

await app.RunAsync();