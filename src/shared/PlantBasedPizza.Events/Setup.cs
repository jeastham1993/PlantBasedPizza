using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace PlantBasedPizza.Events;

public static class Setup
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var hostName = configuration.GetSection("Messaging")["HostName"];

        if (hostName is null)
        {
            throw new EventBusConnectionException("", "Host name is null");
        }
        
        services.AddSingleton(new RabbitMQConnection(hostName!));
        services.Configure<RabbitMqSettings>(configuration.GetSection("Messaging"));
        services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
        services.AddSingleton<RabbitMqEventSubscriber>();

        return services;
    }
}