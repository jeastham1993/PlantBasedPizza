using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PlantBasedPizza.Recipes.Core;
using PlantBasedPizza.Recipes.Core.CreateRecipe;
using PlantBasedPizza.Recipes.Core.IntegrationEvents;
using PlantBasedPizza.Recipes.Core.OrderCompletedHandler;

namespace PlantBasedPizza.Recipes.Infrastructure;

using MongoDB.Bson.Serialization;

public static class Setup
{
    public static IServiceCollection AddRecipeInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var client = new MongoClient(configuration["DatabaseConnection"]);

        services.AddSingleton(client);
        services.AddDaprClient();

        BsonClassMap.RegisterClassMap<Recipe>(map =>
        {
            map.AutoMap();
            map.MapField("_ingredients");
            map.SetIgnoreExtraElements(true);
            map.SetIgnoreExtraElementsIsInherited(true);
        });

        BsonClassMap.RegisterClassMap<Ingredient>(map =>
        {
            map.AutoMap();
            map.SetIgnoreExtraElements(true);
            map.SetIgnoreExtraElementsIsInherited(true);
        });

        services.AddSingleton<IRecipeRepository, RecipeRepository>();
        services.AddSingleton<CreateRecipeCommandHandler>();
        services.AddSingleton<OrderCompletedHandler>();

        if (!string.IsNullOrEmpty(configuration["DAPR_PUB_SUB_NAME"]))
            services.AddSingleton<IEventPublisher, DaprEventPublisher>();
        else if (!string.IsNullOrEmpty(configuration["AZURE_SERVICE_BUS_CONNECTION_STRING"]))
        {
            services.AddSingleton(new ServiceBusClient(
                configuration["AZURE_SERVICE_BUS_CONNECTION_STRING"],
                new ServiceBusClientOptions
                { 
                    TransportType = ServiceBusTransportType.AmqpWebSockets,
                }));
            services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
        }
        else
            services.AddSingleton<IEventPublisher, NoOpEventPublisher>();


        return services;
    }
}