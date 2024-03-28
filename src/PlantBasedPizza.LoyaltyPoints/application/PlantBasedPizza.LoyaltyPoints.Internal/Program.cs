using PlantBasedPizza.Api.Internal.Services;
using PlantBasedPizza.LoyaltyPoints.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLoyaltyServices(builder.Configuration);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<LoyaltyService>();
app.MapGet("/loyalty/health", () => "Healthy");

app.Run();