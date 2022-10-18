using PlantBasedPizza.Api;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi, new SourceGeneratorLambdaJsonSerializer<ApiSerializationContext>(options =>
{
    options.PropertyNameCaseInsensitive = true;
}));

builder.Services.AddOrderManagerInfrastructure(builder.Configuration);
builder.Services.AddRecipeInfrastructure(builder.Configuration);
builder.Services.AddKitchenInfrastructure(builder.Configuration);
builder.Services.AddDeliveryModuleInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.AddContext<ApiSerializationContext>();
});

var app = builder.Build();

DomainEvents.Container = app.Services;

app.Map("/health", () =>
{
    return "OK";
});

app.Use(async (context, next) =>
{
    var observability = app.Services.GetService<IObservabilityService>();

    var correlationId = string.Empty;

    if (context.Request.Headers.ContainsKey("X-Amzn-Trace-Id"))
    {
        correlationId = context.Request.Headers["X-Amzn-Trace-Id"].ToString();

        context.Request.Headers.Add(CorrelationContext.DefaultRequestHeaderName, correlationId);
    }
    else if (context.Request.Headers.ContainsKey(CorrelationContext.DefaultRequestHeaderName))
    {
        correlationId = context.Request.Headers[CorrelationContext.DefaultRequestHeaderName].ToString();
    }
    else
    {
        correlationId = Guid.NewGuid().ToString();

        context.Request.Headers.Add(CorrelationContext.DefaultRequestHeaderName, correlationId);
    }

    var timer = new System.Timers.Timer();

    timer.Start();

    CorrelationContext.SetCorrelationId(correlationId);

    observability.Info($"Request received to {context.Request.Path.Value}");

    context.Response.Headers.Add(CorrelationContext.DefaultRequestHeaderName, correlationId);

    // Do work that doesn't write to the Response.
    await next.Invoke();

    timer.Stop();

    var routesToIgnore = new string[3]
    {
        "health",
        "faivcon.ico",
        "swagger"
    };

    var pathRoute = context.Request.Path.Value.Split('/');

    if (routesToIgnore.Contains(pathRoute[1]) == false)
    {
        observability.PutMetric(pathRoute[1], $"{pathRoute[^1]}-Latency", timer.Interval).Wait();
    }
});

app.MapControllers();

app.Run();