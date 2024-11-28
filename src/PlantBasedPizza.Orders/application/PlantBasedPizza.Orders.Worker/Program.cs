using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Orders.Worker;
using PlantBasedPizza.Orders.Worker.Handlers;
using PlantBasedPizza.Orders.Worker.Notifications;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Caching;
using PlantBasedPizza.Shared.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;

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
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdClaimUserProvider>();
builder.Services.AddSingleton<IUserNotificationService, UserNotificationService>();

var serviceName = "OrdersWorker";

builder.Services
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddOrderManagerInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Auth:Issuer"],
        ValidAudience = builder.Configuration["Auth:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Auth:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // breakpoints never hit...
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrWhiteSpace(accessToken) &&
                path.ToString().Contains("notifications", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCaching(builder.Configuration);

builder.Services.AddSingleton<DriverCollectedOrderEventHandler>();
builder.Services.AddSingleton<DriverDeliveredOrderEventHandler>();
builder.Services.AddSingleton<OrderBakedEventHandler>();
builder.Services.AddSingleton<OrderPreparingEventHandler>();
builder.Services.AddSingleton<OrderPrepCompleteEventHandler>();
builder.Services.AddSingleton<OrderQualityCheckedEventHandler>();
builder.Services.AddSingleton<PaymentSuccessEventHandler>();

builder.Services.AddHostedService<OutboxWorker>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/orders/health", () => "Healthy").AllowAnonymous();
app.MapHub<OrderNotificationsHub>("/notifications/orders").AllowAnonymous();

app.MapSubscribeHandler()
    .AllowAnonymous();
app.UseCloudEvents();
app.AddEventHandlers();

appLogger.LogInformation("Running!");

await app.RunAsync();