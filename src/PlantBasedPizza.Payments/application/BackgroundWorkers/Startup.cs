using BackgroundWorkers.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared;

namespace BackgroundWorkers;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        
        services
            .AddSharedInfrastructure(configuration, "Payments")
            .AddMessaging(configuration);

        services.AddSingleton<PaymentService>();
    }
}