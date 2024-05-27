using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;
using PlantBasedPizza.Shared;

namespace BackgroundWorkers;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var parameterProvider = AWS.Lambda.Powertools.Parameters.ParametersManager.SsmProvider.WithDecryption();
        var databaseConnectionParam =
            parameterProvider.Get<string>(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_PARAM_NAME")); 
            
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        
        services
            .AddSharedInfrastructure(configuration, Environment.GetEnvironmentVariable("SERVICE_NAME"))
            .AddMessaging(configuration)
            .AddLoyaltyServices(configuration, Environment.GetEnvironmentVariable("SERVICE_NAME"), databaseConnectionParam);

        services.AddSingleton<AddLoyaltyPointsCommandHandler>();
    }
}