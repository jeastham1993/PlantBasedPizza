using Amazon.EventBridge;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PlantBasedPizza.Events;

public static class Setup
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EventBridgeSettings>(configuration.GetSection("Messaging"));
        
        services.AddSingleton(new AmazonEventBridgeClient());
        services.AddSingleton(new AmazonSQSClient());
        services.AddSingleton<IEventPublisher, EventBridgeEventPublisher>();
        services.AddSingleton<SqsEventSubscriber>();

        return services;
    }
}