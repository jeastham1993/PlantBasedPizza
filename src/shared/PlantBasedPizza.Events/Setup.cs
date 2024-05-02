using Amazon.EventBridge;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PlantBasedPizza.Events;

public static class Setup
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EventBridgeSettings>(configuration.GetSection("Messaging"));
        
        services.AddSingleton(new AmazonEventBridgeClient());
        services.AddSingleton<IEventPublisher, EventBridgeEventPublisher>();

        return services;
    }
}