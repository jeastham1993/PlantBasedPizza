using BackgroundWorkers.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Shared;

namespace BackgroundWorkers;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        
        services
            .AddSharedInfrastructure(configuration, Environment.GetEnvironmentVariable("SERVICE_NAME"))
            .AddMessaging(configuration)
            .AddOrderManagerInfrastructure(configuration);

        services.AddSingleton<DriverCollectedOrderEventHandler>();
        services.AddSingleton<DriverDeliveredOrderEventHandler>();
        services.AddSingleton<OrderBakedEventHandler>();
        services.AddSingleton<OrderPreparingEventHandler>();
        services.AddSingleton<OrderPrepCompleteEventHandler>();
        services.AddSingleton<OrderQualityCheckedEventHandler>();
        
        services.AddSingleton<PaymentSuccessfulEventHandler>();
    }
}
