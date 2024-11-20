using Microsoft.AspNetCore.Builder;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Kitchen.Worker;
using PlantBasedPizza.Kitchen.Worker.Handlers;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Configuration
    .AddEnvironmentVariables();

builder.Services.AddDaprClient();

var serviceName = "KitchenWorker";

builder.Services
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddMessaging(builder.Configuration)
    .AddKitchenInfrastructure(builder.Configuration);

builder.Services.AddSingleton<OrderSubmittedEventHandler>();

var app = builder.Build();

app.MapSubscribeHandler();
app.UseCloudEvents();
app.AddOrderSubmittedProcessing();

await app.RunAsync();