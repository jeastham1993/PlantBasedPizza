using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.Kitchen.Api;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;
using Saunter;
using Saunter.AsyncApiSchema.v2;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.AddLoggerConfigs();

var applicationName = "KitchenApi";

builder.Services.AddSharedInfrastructure(builder.Configuration, applicationName)
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

app.UseCors(CorsSettings.ALLOW_ALL_POLICY_NAME);

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.UseSharedMiddleware();

app.MapGet("/kitchen/health", () => "Healthy");

const string STAFF_ROLE_NAME = "staff";
const string ADMIN_ROLE_NAME = "admin";

app.MapGet("/kitchen/new", Endpoints.GetNew)
    .RequireAuthorization(options => options.RequireRole(STAFF_ROLE_NAME, ADMIN_ROLE_NAME));
app.MapPut("/kitchen/{orderIdentifier}/preparing", Endpoints.MarkPreparing)
    .RequireAuthorization(options => options.RequireRole(STAFF_ROLE_NAME, ADMIN_ROLE_NAME));
app.MapGet("/kitchen/prep", Endpoints.GetPrep)
    .RequireAuthorization(options => options.RequireRole(STAFF_ROLE_NAME, ADMIN_ROLE_NAME));
app.MapPut("/kitchen/{orderIdentifier}/prep-complete", Endpoints.MarkPrepComplete)
    .RequireAuthorization(options => options.RequireRole(STAFF_ROLE_NAME, ADMIN_ROLE_NAME));
app.MapGet("/kitchen/baking", Endpoints.GetBaking)
    .RequireAuthorization(options => options.RequireRole(STAFF_ROLE_NAME, ADMIN_ROLE_NAME));
app.MapPut("/kitchen/{orderIdentifier}/bake-complete", Endpoints.MarkBakeComplete)
    .RequireAuthorization(options => options.RequireRole(STAFF_ROLE_NAME, ADMIN_ROLE_NAME));
app.MapGet("/kitchen/quality-check", Endpoints.GetAwaitingQualityCheck)
    .RequireAuthorization(options => options.RequireRole(STAFF_ROLE_NAME, ADMIN_ROLE_NAME));
app.MapPut("/kitchen/{orderIdentifier}/quality-check", Endpoints.MarkQualityChecked)
    .RequireAuthorization(options => options.RequireRole(STAFF_ROLE_NAME, ADMIN_ROLE_NAME));

app.UseEndpoints(endpoints =>
{
    if (generateAsyncApi)
    {
        endpoints.MapAsyncApiDocuments();
        endpoints.MapAsyncApiUi();
    }
});

await app.RunAsync();