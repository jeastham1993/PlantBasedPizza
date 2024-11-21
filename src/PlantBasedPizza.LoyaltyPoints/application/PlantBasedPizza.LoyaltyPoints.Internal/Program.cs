using PlantBasedPizza.LoyaltyPoints.Internal.Services;
using PlantBasedPizza.LoyaltyPoints.Shared;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.AddLoggerConfigs();

var serviceName = "LoyaltyInternalApi";

builder.Services.AddLoyaltyServices(builder.Configuration, serviceName);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<LoyaltyService>();
app.MapGet("/loyalty/health", () => "Healthy");

app.Run();