using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared;
using PlantBasedPizza.LoyaltyPoints.Worker;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDaprClient();

builder.Services
    .AddLoyaltyServices(builder.Configuration, "LoyaltyPointsWorker")
    .AddMessaging(builder.Configuration);

var app = builder.Build();

app.MapGet("/loyalty/health", () => "Healthy");

app.MapSubscribeHandler();
app.UseCloudEvents();
app.AddLoyaltyPointsEventHandler();

app.Run();