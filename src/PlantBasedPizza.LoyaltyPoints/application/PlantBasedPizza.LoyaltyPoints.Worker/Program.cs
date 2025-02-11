using PlantBasedPizza.LoyaltyPoints.Shared;
using PlantBasedPizza.LoyaltyPoints.Worker;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.AddLoggerConfigs();

builder.Services.AddDaprClient();

var serviceName = "LoyaltyWorker";

builder.Services
    .AddLoyaltyServices(builder.Configuration, serviceName);

builder.Services.AddSingleton<IDeadLetterRepository, DeadLetterRepository>();

var app = builder.Build();

app.MapGet("/loyalty/health", () => "Healthy");

app.MapSubscribeHandler();
app.UseCloudEvents();

app.MapPost("/order-completed", EventHandlers.HandleOrderCompletedEvent);
app.MapPost("/errors", EventHandlers.HandleDeadLetterMessage);

await app.RunAsync();