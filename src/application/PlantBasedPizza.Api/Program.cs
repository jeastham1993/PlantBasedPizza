using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddSharedInfrastructure();
// Add services to the container.
builder.Services.AddOrderManagerInfrastructure(builder.Configuration);
builder.Services.AddRecipeInfrastructure(builder.Configuration);
builder.Services.AddKitchenInfrastructure(builder.Configuration);
builder.Services.AddDeliveryModuleInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

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

    var pathRoute = context.Request.Path.Value.Split('/')[1];

    observability.PutMetric(pathRoute, $"{context.Request.Path.Value.Replace('/', '-')}-Latency", timer.Interval).Wait();
    
    observability.EndTraceSegment();
});

app.Run();
