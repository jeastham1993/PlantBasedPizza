using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddDeliveryModuleInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
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

            services.AddSingleton(new AmazonDynamoDBClient());
            
            services.AddTransient<IDeliveryRequestRepository, DeliveryRequestRepository>();
            services.AddTransient<Handles<OrderReadyForDeliveryEvent>, OrderReadyForDeliveryEventHandler>();

            return services;
        }
    }
}