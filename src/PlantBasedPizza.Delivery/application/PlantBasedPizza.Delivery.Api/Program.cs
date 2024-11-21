using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Deliver.Infrastructure.IntegrationEvents;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;
using Saunter;
using Saunter.AsyncApiSchema.v2;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.AddLoggerConfigs();

var applicationName = "DeliveryApi";

builder.Services.AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddDeliveryInfrastructure(builder.Configuration)
    .AddControllers();

var generateAsyncApi = builder.Configuration["Messaging:UseAsyncApi"] == "Y";

if (generateAsyncApi)
{
    builder.Services.AddAsyncApiSchemaGeneration(options =>
    {
        options.AssemblyMarkerTypes = new[] {typeof(DeliveryEventPublisher)};

        options.AsyncApi = new AsyncApiDocument
        {
            Info = new Info("PlantBasedPizza Delivery API", "1.0.0")
            {
                Description = "The delivery API manages orders that are deing delivered to customers.",
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

if (generateAsyncApi)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapAsyncApiDocuments();
        endpoints.MapAsyncApiUi();
    });   
}

app.Run();
