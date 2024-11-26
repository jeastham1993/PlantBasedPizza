using PlantBasedPizza.Payments;
using PlantBasedPizza.Payments.Services;
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

// Add services to the container.
builder.Services.AddGrpc();

builder.Services
    .AddSharedInfrastructure(builder.Configuration, "PaymentApi");

builder.Services.AddDaprClient();

builder.Services.AddSingleton<APIKeyProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.MapGrpcService<PaymentService>();
app.MapGet("/payments/health", () => "Healthy");

appLogger.LogInformation("Running!");

await app.RunAsync();