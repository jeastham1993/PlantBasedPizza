using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.GetDelivery;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Deliver.Infrastructure.IntegrationEvents;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    using MongoDB.Bson.Serialization;

    public static class Setup
    {
        public static IServiceCollection AddDeliveryInfrastructure(this IServiceCollection services,
            IConfiguration configuration, string overrideDatabaseConnection = null)
        {
            var client = new MongoClient(overrideDatabaseConnection ?? configuration["DatabaseConnection"]);

            services.AddSingleton(client);
            
            BsonClassMap.RegisterClassMap<DeliveryRequest>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            BsonClassMap.RegisterClassMap<Address>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            services.AddSingleton<IDeliveryRequestRepository, DeliveryRequestRepository>();
            services.AddSingleton<IDeliveryEventPublisher, DeliveryEventPublisher>();
            services.AddSingleton<OrderReadyForDeliveryEventHandler>();
            services.AddSingleton<GetDeliveryQueryHandler>();

            return services;
        }
    }
}