using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Orders.Internal.Services;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.AddLoggerConfigs();

var applicationName = "OrdersInternal";

builder.Services.AddOrderManagerInfrastructure(builder.Configuration)
    .AddSharedInfrastructure(builder.Configuration, applicationName);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrdersService>();
app.MapGet("/orders/health", () => "Healthy");

app.Run();