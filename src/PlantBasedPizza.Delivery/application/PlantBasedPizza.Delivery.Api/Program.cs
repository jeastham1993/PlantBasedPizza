using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.Deliver.Core.OrderReadyForDelivery;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Delivery.Api;
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

builder.Services.AddAsyncApiDocs(builder.Configuration, [typeof(DeliveryEventPublisher), typeof(OrderReadyForDeliveryEventHandler)], "DeliveryService");

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

app.UseCors(CorsSettings.ALLOW_ALL_POLICY_NAME);

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.UseSharedMiddleware();

app.MapGet("/delivery/{orderIdentifier}/status", Endpoints.GetOrderStatus)
    .RequireAuthorization(options => options.RequireRole("user"));
app.MapGet("/delivery/awaiting-collection", Endpoints.GetAwaitingCollection)
    .RequireAuthorization(options => options.RequireRole("staff", "admin"));
app.MapGet("/delivery/driver/{driverName}/orders", Endpoints.GetOrdersForDriver)
    .RequireAuthorization(options => options.RequireRole("staff", "admin"));
app.MapPost("/delivery/assign", Endpoints.AssignToDriver)
    .RequireAuthorization(options => options.RequireRole("staff", "admin"));
app.MapPost("/delivery/delivered", Endpoints.MarkOrderDelivered)
    .RequireAuthorization(options => options.RequireRole("driver"));

app.UseAsyncApi();

await app.RunAsync();
