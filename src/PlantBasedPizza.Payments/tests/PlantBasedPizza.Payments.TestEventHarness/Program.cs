using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PlantBasedPizza.Payments.TestEventHarness;

var builder = WebApplication.CreateBuilder(args);
builder
    .Configuration
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton(new InMemoryEventMonitor());

var otel = builder.Services.AddOpenTelemetry();
otel.ConfigureResource(resource => resource
    .AddService(serviceName: "PaymentsEventTestHarness"));

otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation(options =>
    {
        options.Filter = new Func<HttpContext, bool>(httpContext =>
        {
            try
            {
                if (httpContext.Request.Path.Value.Contains("/notifications")) return false;

                return true;
            }
            catch
            {
                return true;
            }
        });
    });
    tracing.AddGrpcClientInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddSource("PaymentsEventTestHarness");

    tracing.AddOtlpExporter(otlpOptions =>
    {
        otlpOptions.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
    });
});

builder.Services.AddDaprClient();

var app = builder.Build();

app.UseRouting();

app.MapSubscribeHandler();
app.UseCloudEvents();

app.MapGet("/events/{orderIdentifier}", Handlers.GetEventsForEntity);
app.MapPost("/events/take-payment", Handlers.SendTakePaymentCommand);

app.MapPost("/payment-success", Handlers.HandlePaymentSuccessfulEvent);
app.MapPost("/payment-failed", Handlers.HandlePaymentFailedEvent);

await app.RunAsync();

public partial class Program
{
}