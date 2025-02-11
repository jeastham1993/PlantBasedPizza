using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using PlantBasedPizza.Kitchen.ACL;
using PlantBasedPizza.Kitchen.Core.OrderConfirmed;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Configuration
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

builder.Services.AddDaprClient();
builder.Services.AddSingleton<Idempotency, CachedIdempotencyService>();
builder.Services.AddSingleton<EventAdapter>();

builder.Services
    .AddSharedInfrastructure(builder.Configuration, ApplicationDefaults.ServiceName, disableContextPropagation: true)
    .AddKitchenInfrastructure(builder.Configuration);

builder.Services.AddSingleton<OrderConfirmedEventHandler>();

var app = builder.Build();

app.MapSubscribeHandler();
app.UseCloudEvents();

app.MapPost("/order-confirmed", EventHandlers.HandleOrderConfirmedEvent);
app.MapPost("/errors", EventHandlers.HandleDeadLetterMessage);

appLogger.LogInformation("Running!");

await app.RunAsync();