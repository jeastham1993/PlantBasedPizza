using System.Timers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    var timer = new System.Timers.Timer();
    
    timer.Start();
    
    var observability = app.Services.GetService<IObservabilityService>();
    
    observability.Info($"Request received to {context.Request.Path.Value}");
    
    observability.StartTraceSegment(context.Request.Path.Value);
    
    // Do work that doesn't write to the Response.
    await next.Invoke();
    
    timer.Stop();

    var pathRoute = context.Request.Path.Value.Split('/')[1];

    observability.PutMetric(pathRoute, $"{context.Request.Path.Value.Replace('/', '-')}-Latency", timer.Interval).Wait();
    
    observability.EndTraceSegment();
});

app.Run();