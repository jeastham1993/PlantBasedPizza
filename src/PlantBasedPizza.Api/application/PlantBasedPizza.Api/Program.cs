using System.Threading.RateLimiting;
using Amazon;
using Amazon.SimpleSystemsManagement;
using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.Api;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);
var logger = Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();
builder.Host.UseSerilog((_, config) =>
{
    config.MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter());
});

builder
    .Configuration
    .AddEnvironmentVariables();

var overrideConnectionString = Environment.GetEnvironmentVariable("OVERRIDE_CONNECTION_STRING");
var connectionStringParameterName = Environment.GetEnvironmentVariable("ConnectionStringParameterName");

if (!string.IsNullOrEmpty(connectionStringParameterName))
{
    var ssmClient = new AmazonSimpleSystemsManagementClient();
    var parameter = await ssmClient.GetParameterAsync(new Amazon.SimpleSystemsManagement.Model.GetParameterRequest
    {
        Name = connectionStringParameterName,
        WithDecryption = true
    });
    overrideConnectionString = parameter.Parameter.Value;
    logger.Information($"Overriding connection string to: {overrideConnectionString}");
}

builder.Services.AddLambda(logger);

// Connect to a PostgreSQL database.
builder.Services.AddOrderManagerInfrastructure(builder.Configuration, overrideConnectionString)
    .AddRecipeInfrastructure(builder.Configuration, logger, overrideConnectionString)
    .AddKitchenInfrastructure(builder.Configuration, overrideConnectionString)
    .AddDeliveryModuleInfrastructure(builder.Configuration, overrideConnectionString)
    .AddSharedInfrastructure(builder.Configuration, "PlantBasedPizza")
    .AddHttpClient();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Request.Headers.Host.ToString(),
            partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 60,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddControllers();

var app = builder.Build();

Amazon.Lambda.Core.SnapshotRestore.RegisterBeforeSnapshot(async () =>
{
    logger.Information("Before snapshot");

    var myDbContext = app.Services.GetRequiredService<OrderManagerDbContext>();
            
    logger.Information("Before snapshot finished");
});

app.UseCors("AllowAll");

DomainEvents.Container = app.Services;

app.Map("/health", async () =>
{
    logger.Information("Health check requested");
    
    var ordersDbContext = app.Services.GetRequiredService<OrderManagerDbContext>();
    var ordersConnectionState = await ordersDbContext.Database.CanConnectAsync();
    var recipesDbContext = app.Services.GetRequiredService<RecipesDbContext>();
    var recipesConnectionState = await recipesDbContext.Database.CanConnectAsync();
    var deliveryDbContext = app.Services.GetRequiredService<DeliveryDbContext>();
    var deliveryConnectionState = await deliveryDbContext.Database.CanConnectAsync();
    var kitchenDbContext = app.Services.GetRequiredService<KitchenDbContext>();
    var kitchenConnectionState = await kitchenDbContext.Database.CanConnectAsync();
    
    logger.Information("Healthcheck complete: statuses are {ordersState}, {recipesState}, {deliveryState}, {kitchenState}",
        ordersConnectionState, recipesConnectionState, deliveryConnectionState, kitchenConnectionState);
    
    return Results.Ok(new
    {
        ordersState = ordersConnectionState,
        recipesState = recipesConnectionState,
        deliveryState = deliveryConnectionState,
        kitchenState = kitchenConnectionState
    });
});

app.MapGet("/utils/__migrate", async (
    OrderManagerDbContext ordersDbContext,
    RecipesDbContext recipesDbContext,
    DeliveryDbContext deliveryDbContext,
    KitchenDbContext kitchenDbContext) =>
{
    logger.Information("DB migration requested");
    
    await ordersDbContext.Database.MigrateAsync();
    await recipesDbContext.Database.MigrateAsync();
    await deliveryDbContext.Database.MigrateAsync();
    await kitchenDbContext.Database.MigrateAsync();
    
    return Results.Ok("OK");
});

app.Use(async (context, next) =>
{
    var observability = app.Services.GetService<IObservabilityService>();

    var correlationId = string.Empty;

    if (context.Request.Headers.ContainsKey(CorrelationContext.DefaultRequestHeaderName))
    {
        correlationId = context.Request.Headers[CorrelationContext.DefaultRequestHeaderName].ToString();
    }
    else
    {
        correlationId = Guid.NewGuid().ToString();

        context.Request.Headers.Append(CorrelationContext.DefaultRequestHeaderName, correlationId);
    }

    CorrelationContext.SetCorrelationId(correlationId);

    observability?.Info($"Request received to {context.Request.Path.Value}");

    context.Response.Headers.Append(CorrelationContext.DefaultRequestHeaderName, correlationId);

    // Do work that doesn't write to the Response.
    await next.Invoke();
});

app.MapControllers();

app.Run();