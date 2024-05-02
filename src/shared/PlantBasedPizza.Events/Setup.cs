using Amazon.EventBridge;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace PlantBasedPizza.Events;

public static class Setup
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var hostName = configuration["Messaging:HostName"];

        if (hostName is null)
        {
            throw new EventBusConnectionException("", "Host name is null");
        }
        
        services.Configure<RabbitMqSettings>(configuration.GetSection("Messaging"));

        if (hostName == "eventbridge")
        {
            services.AddSingleton(new AmazonEventBridgeClient());
            services.AddSingleton<IEventPublisher, EventBridgeEventPublisher>();
        }
        else
        {
            services.AddSingleton(new RabbitMQConnection(hostName!));
            services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
            services.AddSingleton<RabbitMqEventSubscriber>();   
        }

        return services;
    }
}