using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PlantBasedPizza.Events;

public static class Setup
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection("Messaging"));
        services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
        services.AddSingleton<RabbitMqEventSubscriber>();

        return services;
    }
}