using PlantBasedPizza.LoyaltyPoints.Shared;
using PlantBasedPizza.LoyaltyPoints.Worker;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.AddLoggerConfigs();

builder.Services.AddDaprClient();

var serviceName = "LoyaltyWorker";

builder.Services
    .AddLoyaltyServices(builder.Configuration, serviceName);

var app = builder.Build();

app.MapGet("/loyalty/health", () => "Healthy");

app.MapSubscribeHandler();
app.UseCloudEvents();
app.AddLoyaltyPointsEventHandler();

await app.RunAsync();