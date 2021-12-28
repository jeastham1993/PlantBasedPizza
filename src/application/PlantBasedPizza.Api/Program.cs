using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;
using Microsoft.OpenApi.Models;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Core.ViewModels;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc = ApplicationLogger.BuildLoggerConfiguration();
});

// Add services to the container.
builder.Services.AddOrderManagerInfrastructure(builder.Configuration);
builder.Services.AddRecipeInfrastructure(builder.Configuration);
builder.Services.AddKitchenInfrastructure(builder.Configuration);
builder.Services.AddDeliveryModuleInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Plant Based Pizza API",
        Description = "The API for the Plant Based Pizza API",
        Contact = new OpenApiContact
        {
            Name = "James Eastham",
            Url = new Uri("https://jameseastham.co.uk")
        },
    });

    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PlantBasedPizza.Deliver.Infrastructure.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PlantBasedPizza.Kitchen.Infrastructure.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PlantBasedPizza.OrderManager.Infrastructure.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PlantBasedPizza.Recipes.Infrastructure.xml"));
});

builder.Services.AddAsyncApiSchemaGeneration(options =>
{
    // Specify example type(s) from assemblies to scan.
    options.AssemblyMarkerTypes = new[] {typeof(DriverCollectedOrderEvent), typeof(Order), typeof(DeliveryRequest), typeof(KitchenRequest)};

    // Build as much (or as little) of the AsyncApi document as you like.
    // Saunter will generate Channels, Operations, Messages, etc, but you
    // may want to specify Info here.
    options.AsyncApi = new AsyncApiDocument
    {
        Info = new Info("Plant Based Pizza API", "1.0.0")
        {
            Description = "The Plant Based Pizza event messages..",
        },
    };
});

var app = builder.Build();

app.UseXRay("PlantBasedPizza.Api");

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapAsyncApiDocuments();
    endpoints.MapAsyncApiUi();
});

app.MapControllers();

DomainEvents.Container = app.Services;

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
    
    observability.StartTraceSegment(context.Request.Path.Value);

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
    
        observability.EndTraceSegment();
    }
});

app.Run();
