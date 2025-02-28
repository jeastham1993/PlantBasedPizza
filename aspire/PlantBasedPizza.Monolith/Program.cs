using System.Collections.Immutable;
using Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Monolith;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder
    .AddMongoDB("mongodb")
    .WithDataVolume()
    .AddDatabase("PlantBasedPizza");

var redis = builder.AddRedis("cache", 6379);

var observability = builder.AddLocalObservability();

var daprScheduler = builder.AddDaprWithScheduler();

var loyaltyService = builder.AddMockedLoyaltyService();

var application = builder.AddApplication(db, observability);

// Workaround for https://github.com/dotnet/aspire/issues/2219
if (builder.Configuration.GetValue<string>("DAPR_CLI_PATH") is { } daprCliPath)
{
    builder.Services.Configure<DaprOptions>(options =>
    {
        options.DaprPath = daprCliPath;
    });
}

await builder.Build().RunAsync();