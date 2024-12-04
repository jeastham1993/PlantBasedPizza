using System.Text;
using System.Text.Json;
using FastEndpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PlantBasedPizza.Account.Api.Configurations;
using PlantBasedPizza.Account.Api.Core;
using PlantBasedPizza.Shared;
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

var serviceName = "AccountApi";

builder.Services
    .AddFastEndpoints()
    .AddAuthConfigs(appLogger, builder)
    .AddServiceConfigs(appLogger, builder)
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddHealthChecks()
    .AddMongoDb(builder.Configuration["DatabaseConnection"]);

var app = builder.Build();

app.MapHealthChecks("/account/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
});

app.UseCors(CorsSettings.ALLOW_ALL_POLICY_NAME);

app.UseAuthentication()
    .UseAuthorization()
    .UseFastEndpoints(options =>
    {
        options.Endpoints.RoutePrefix = "account";
        options.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

var accountRepository = app.Services.GetRequiredService<IUserAccountRepository>();
await accountRepository.SeedInitialUser();

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
