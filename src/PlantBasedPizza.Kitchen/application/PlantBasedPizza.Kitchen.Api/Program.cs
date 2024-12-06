using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.Kitchen.Api;
using PlantBasedPizza.Kitchen.Core.OrderConfirmed;
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
    .AddHealthChecks()
    .AddMongoDb(builder.Configuration["DatabaseConnection"]);

builder.Services.AddAsyncApiDocs(builder.Configuration,
    [typeof(KitchenEventPublisher), typeof(OrderConfirmedEventHandler)], "KitchenService");

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

app.MapHealthChecks("/kitchen/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
});

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

app.UseAsyncApi();

await app.RunAsync();

static Task WriteHealthCheckResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var options = new JsonWriterOptions { Indented = true };

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("status", healthReport.Status.ToString());
        jsonWriter.WriteStartObject("results");

        foreach (var healthReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject(healthReportEntry.Key);
            jsonWriter.WriteString("status",
                healthReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("description",
                healthReportEntry.Value.Description);
            jsonWriter.WriteStartObject("data");

            foreach (var item in healthReportEntry.Value.Data)
            {
                jsonWriter.WritePropertyName(item.Key);

                JsonSerializer.Serialize(jsonWriter, item.Value,
                    item.Value?.GetType() ?? typeof(object));
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();
    }

    return context.Response.WriteAsync(
        Encoding.UTF8.GetString(memoryStream.ToArray()));
}