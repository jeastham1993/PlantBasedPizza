using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Kitchen.Worker;
using PlantBasedPizza.Kitchen.Worker.Handlers;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Configuration
    .AddEnvironmentVariables();
builder.AddLoggerConfigs();

builder.Services.AddDaprClient();

var serviceName = "KitchenWorker";

builder.Services
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddKitchenInfrastructure(builder.Configuration);

builder.Services.AddSingleton<OrderSubmittedEventHandler>();

var app = builder.Build();

app.MapSubscribeHandler();
app.UseCloudEvents();
app.AddOrderSubmittedProcessing();

await app.RunAsync();