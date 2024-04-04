using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared;
using PlantBasedPizza.LoyaltyPoints.Worker;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddLoyaltyServices(builder.Configuration, "LoyaltyPointsWorker")
    .AddMessaging(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapGet("/loyalty/health", () => "Healthy");

app.Run();