using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;
using Saunter;
using Saunter.AsyncApiSchema.v2;
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
});

builder.Services.AddAuthorization();

var applicationName = "OrdersApi";

builder.Services.AddOrderManagerInfrastructure(builder.Configuration)
    .AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddAsyncApiDocs(builder.Configuration, [typeof(OrderEventPublisher)], "OrdersService");

builder.Services.AddHttpClient();

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(CorsSettings.ALLOW_ALL_POLICY_NAME);

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.UseSharedMiddleware();

var orderManagerHealthChecks = app.Services.GetRequiredService<OrderManagerHealthChecks>();

app.Map("/order/health", async () =>
{
    var healthCheckResult = await orderManagerHealthChecks.Check();
    
    return Results.Ok(healthCheckResult);
});

app.MapControllers();
app.UseAsyncApi();

appLogger.LogInformation("Running!");

await app.RunAsync();