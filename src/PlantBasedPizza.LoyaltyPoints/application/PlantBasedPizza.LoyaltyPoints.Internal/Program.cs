using PlantBasedPizza.LoyaltyPoints.Internal.Services;
using PlantBasedPizza.LoyaltyPoints.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLoyaltyServices(builder.Configuration, "LoyaltyPointsInternal");

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<LoyaltyService>();
app.MapGet("/loyalty/health", () => "Healthy");

app.Run();