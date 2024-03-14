using System.Diagnostics;
using MongoDB.Driver;

using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);
builder
    .Configuration
    .AddEnvironmentVariables();

var client = new MongoClient(builder.Configuration["DatabaseConnection"]);

builder.Services.AddSingleton(client);

builder.Services.AddOrderManagerInfrastructure(builder.Configuration);
builder.Services.AddRecipeInfrastructure(builder.Configuration);
builder.Services.AddKitchenInfrastructure(builder.Configuration);
builder.Services.AddDeliveryModuleInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration, "PlantBasedPizza");
builder.Services.AddHttpClient();

builder.Services.AddControllers();

var app = builder.Build();

DomainEvents.Container = app.Services;

var httpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient();

app.Map("/health", async () =>
{
    try
    {
        var res = await httpClient.GetAsync($"{app.Configuration["Services:Loyalty"]}/loyalty/health");

        if (!res.IsSuccessStatusCode)
        {
            return Results.Problem("Loyalty points service inactive");
        }

        Activity.Current?.AddTag("loyalty.healthy", res.IsSuccessStatusCode);
    }
    catch (Exception)
    {
        Activity.Current?.AddTag("loyalty.healthy", false);
        
        return Results.Problem("Loyalty points service inactive");
    }
    
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