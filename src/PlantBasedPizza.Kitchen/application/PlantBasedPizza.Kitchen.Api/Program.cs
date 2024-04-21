using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Kitchen.Infrastructure.IntegrationEvents;
using PlantBasedPizza.Shared;
using Saunter;
using Saunter.AsyncApiSchema.v2;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var applicationName = "KitchenApi";

builder.Services.AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddMessaging(builder.Configuration)
    .AddKitchenInfrastructure(builder.Configuration)
    .AddControllers();

var generateAsyncApi = builder.Configuration["Messaging:UseAsyncApi"] == "Y";

if (generateAsyncApi)
{
    builder.Services.AddAsyncApiSchemaGeneration(options =>
    {
        options.AssemblyMarkerTypes = new[] {typeof(KitchenEventPublisher)};

        options.AsyncApi = new AsyncApiDocument
        {
            Info = new Info("PlantBasedPizza Delivery API", "1.0.0")
            {
                Description = "The kitchen API manages orders as they are being cooked.",
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

var app = builder.Build();

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/kitchen/health", () => "Healthy");

if (generateAsyncApi)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapAsyncApiDocuments();
        endpoints.MapAsyncApiUi();
    });   
}

app.Run();