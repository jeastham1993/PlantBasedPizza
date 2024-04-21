using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;
using PlantBasedPizza.Shared;
using Saunter;
using Saunter.AsyncApiSchema.v2;

var builder = WebApplication.CreateBuilder(args);
builder
    .Configuration
    .AddEnvironmentVariables();

var generateAsyncApi = builder.Configuration["Messaging:UseAsyncApi"] == "Y";

if (generateAsyncApi)
{
    builder.Services.AddAsyncApiSchemaGeneration(options =>
    {
        options.AssemblyMarkerTypes = new[] {typeof(OrderEventPublisher)};

        options.AsyncApi = new AsyncApiDocument
        {
            Info = new Info("PlantBasedPizza Orders API", "1.0.0")
            {
                Description = "The orders API allows orders to be placed.",
            },
        };
    });   
}

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

builder.Services.AddOrderManagerInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration, "PlantBasedPizza")
    .AddMessaging(builder.Configuration);

builder.Services.AddHttpClient();

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

var orderManagerHealthChecks = app.Services.GetRequiredService<OrderManagerHealthChecks>();

app.Map("/order/health", async () =>
{
    var healthCheckResult = await orderManagerHealthChecks.Check();
    
    return Results.Ok(healthCheckResult);
});

app.MapControllers();

if (generateAsyncApi)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapAsyncApiDocuments();
        endpoints.MapAsyncApiUi();
    });   
}

app.Run();