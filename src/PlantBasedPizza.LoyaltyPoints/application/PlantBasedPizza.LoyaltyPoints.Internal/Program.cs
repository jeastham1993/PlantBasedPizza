using PlantBasedPizza.LoyaltyPoints.Internal.Services;
using PlantBasedPizza.LoyaltyPoints.Shared;
using PlantBasedPizza.Shared;
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

var serviceName = "LoyaltyInternalApi";

builder.Services
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddLoyaltyServices(builder.Configuration, serviceName);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDaprClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<LoyaltyService>();
app.MapGrpcService<AppCallbackService>();
app.MapGet("/loyalty/health", () => "Healthy");

appLogger.LogInformation("Running!");

await app.RunAsync();