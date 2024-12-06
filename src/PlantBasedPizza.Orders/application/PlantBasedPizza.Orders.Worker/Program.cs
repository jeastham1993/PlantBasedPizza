using Microsoft.AspNetCore.SignalR;
using PlantBasedPizza.OrderManager.Core.DriverCollectedOrder;
using PlantBasedPizza.OrderManager.Core.DriverDeliveredOrder;
using PlantBasedPizza.OrderManager.Core.OrderBaked;
using PlantBasedPizza.OrderManager.Core.OrderPreparing;
using PlantBasedPizza.OrderManager.Core.OrderPrepComplete;
using PlantBasedPizza.OrderManager.Core.OrderQualityChecked;
using PlantBasedPizza.OrderManager.Core.PaymentSuccess;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.OrderManager.Infrastructure.Notifications;
using PlantBasedPizza.Orders.Worker;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Authentication;
using PlantBasedPizza.Shared.Caching;
using PlantBasedPizza.Shared.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;
using Temporalio.Extensions.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

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

builder.Services
    .AddSharedInfrastructure(builder.Configuration, ApplicationDefaults.ServiceName, new[]
    {
        TracingInterceptor.ClientSource.Name,
        TracingInterceptor.WorkflowsSource.Name,
        TracingInterceptor.ActivitiesSource.Name
    })
    .AddOrderManagerInfrastructure(builder.Configuration)
    .ConfigureAuth(builder.Configuration)
    .AddAuthorization()
    .AddCaching(builder.Configuration)
    .AddTemporalWorkflows(builder.Configuration);

builder.Services.AddSingleton<DriverCollectedOrderEventHandler>();
builder.Services.AddSingleton<DriverDeliveredOrderEventHandler>();
builder.Services.AddSingleton<OrderBakedEventHandler>();
builder.Services.AddSingleton<OrderPreparingEventHandler>();
builder.Services.AddSingleton<OrderPrepCompleteEventHandler>();
builder.Services.AddSingleton<OrderQualityCheckedEventHandler>();
builder.Services.AddSingleton<PaymentSuccessEventHandler>();
builder.Services.AddSingleton<Idempotency, CachedIdempotencyService>();
builder.Services.AddHostedService<OutboxWorker>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<OrderNotificationsHub>("/notifications/orders")
    .AllowAnonymous();
app.MapGet("/orders/health", () => "Healthy").AllowAnonymous();
app.MapPost("/payment-success", EventHandlers.HandlePaymentSuccessfulEvent);
app.MapPost("/driver-collected", EventHandlers.HandleDriverCollectedOrderEvent);
app.MapPost("/driver-delivered", EventHandlers.HandleDriverDeliveredOrderEvent);
app.MapPost("/loyalty-updated", EventHandlers.HandleLoyaltyPointsUpdatedEvent);
app.MapPost("/order-baked", EventHandlers.HandleOrderBakedEvent);
app.MapPost("/order-preparing", EventHandlers.HandleOrderPreparingEvent);
app.MapPost("/order-prep-complete", EventHandlers.HandleOrderPrepCompleteEvent);
app.MapPost("/order-quality-checked", EventHandlers.HandleOrderQualityCheckedEvent);
app.MapPost("/event-dead-letter", EventHandlers.HandleDeadLetterMessage);

app.MapSubscribeHandler()
    .AllowAnonymous();
app.UseCloudEvents();

appLogger.LogInformation("Running!");

await app.RunAsync();