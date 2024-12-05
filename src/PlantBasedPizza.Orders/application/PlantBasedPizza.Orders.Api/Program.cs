using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.OrderManager.Infrastructure.HealthChecks;
using PlantBasedPizza.Orders.Api;
using PlantBasedPizza.Orders.Worker.Handlers;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;
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
    .AddAsyncApiDocs(builder.Configuration, 
        [typeof(OrderEventPublisher),
            typeof(DriverDeliveredOrderEventHandler),
            typeof(DriverCollectedOrderEventHandler),
            typeof(OrderBakedEventHandler),
            typeof(OrderPreparingEventHandler),
            typeof(OrderPrepCompleteEventHandler),
            typeof(OrderQualityCheckedEventHandler),
            typeof(PaymentSuccessEventHandler),
        ]
        , "OrdersService");

builder.Services.AddHttpClient()
    .AddHealthChecks()
    .AddCheck<LoyaltyServiceHealthChecks>("LoyaltyService")
    .AddCheck<RecipeServiceHealthCheck>("RecipeService")
    .AddCheck<DeadLetterQueueChecks>("DeadLetterQueue")
    .AddMongoDb(builder.Configuration["DatabaseConnection"]);

var app = builder.Build();

app.UseCors(CorsSettings.ALLOW_ALL_POLICY_NAME)
    .UseAuthentication()
    .UseRouting()
    .UseAuthorization()
    .UseSharedMiddleware();

app.MapHealthChecks("/order/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
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

static Task WriteHealthCheckResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var options = new JsonWriterOptions { Indented = true };

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("status", healthReport.Status.ToString());
        jsonWriter.WriteStartObject("results");

        foreach (var healthReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject(healthReportEntry.Key);
            jsonWriter.WriteString("status",
                healthReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("description",
                healthReportEntry.Value.Description);
            jsonWriter.WriteStartObject("data");

            foreach (var item in healthReportEntry.Value.Data)
            {
                jsonWriter.WritePropertyName(item.Key);

                JsonSerializer.Serialize(jsonWriter, item.Value,
                    item.Value?.GetType() ?? typeof(object));
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();
    }

    return context.Response.WriteAsync(
        Encoding.UTF8.GetString(memoryStream.ToArray()));
}