using System.Text.Json;
using FastEndpoints;
using PlantBasedPizza.Account.Api.Configurations;
using PlantBasedPizza.Account.Api.Core;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared;
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

var serviceName = "PlantBasedPizza.Account";

builder.Services
    .AddFastEndpoints()
    .AddAuthConfigs(appLogger, builder)
    .AddServiceConfigs(appLogger, builder)
    .AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddMessaging(builder.Configuration);

var app = builder.Build();

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
