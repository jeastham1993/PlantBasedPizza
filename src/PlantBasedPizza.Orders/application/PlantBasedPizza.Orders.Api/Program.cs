using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Orders.Api;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Authentication;
using PlantBasedPizza.Shared.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);
builder
    .Configuration
    .AddEnvironmentVariables();

var logger = Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();
builder.AddLoggerConfigs();
var appLogger = new SerilogLoggerFactory(logger)
    .CreateLogger<Program>();

builder.Services.ConfigureAuth(builder.Configuration);

var applicationName = "OrdersApi";

builder.Services.AddOrderManagerInfrastructure(builder.Configuration)
    .AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddAsyncApiDocs(builder.Configuration, [typeof(OrderEventPublisher)], "OrdersService");

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors(CorsSettings.ALLOW_ALL_POLICY_NAME)
    .UseAuthentication()
    .UseRouting()
    .UseAuthorization()
    .UseSharedMiddleware();

app.Map("/order/health", async ([FromServices] OrderManagerHealthChecks healthChecks) =>
{
    var healthCheckResult = await healthChecks.Check();
    
    return Results.Ok(healthCheckResult);
});

app.MapGet("/order", OrderEndpoints.GetForCustomer)
    .RequireAuthorization(options => options.RequireRole("user"));
app.MapGet("/order/{orderIdentifier}/detail", OrderEndpoints.Get)
    .RequireAuthorization(options => options.RequireRole("user"));
app.MapPost("/order/pickup", OrderEndpoints.CreatePickupOrder)
    .RequireAuthorization(options => options.RequireRole("user"));
app.MapPost("/order/deliver", OrderEndpoints.CreateDeliveryOrder)
    .RequireAuthorization(options => options.RequireRole("user"));
app.MapPost("/order/{orderIdentifier}/items", OrderEndpoints.AddItemToOrder)
    .RequireAuthorization(options => options.RequireRole("user"));
app.MapPost("/order/{orderIdentifier}/submit", OrderEndpoints.SubmitOrder)
    .RequireAuthorization(options => options.RequireRole("user"));
app.MapPost("/order/{orderIdentifier}/cancel", OrderEndpoints.CancelOrder)
    .RequireAuthorization(options => options.RequireRole("user", "staff"));
app.MapGet("/order/awaiting-collection", OrderEndpoints.GetAwaitingCollection)
    .RequireAuthorization(options => options.RequireRole("staff"));
app.MapPost("/order/collected", OrderEndpoints.OrderCollected)
    .RequireAuthorization(options => options.RequireRole("staff"));

app.UseAsyncApi();

appLogger.LogInformation("Running!");

await app.RunAsync();