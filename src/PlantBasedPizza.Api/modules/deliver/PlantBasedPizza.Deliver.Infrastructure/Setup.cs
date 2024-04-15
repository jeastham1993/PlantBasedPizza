using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Api.Events;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.GetDelivery;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    using MongoDB.Bson.Serialization;

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

            services.AddHostedService<OrderReadyForDeliveryEventWorker>();
            
            services.AddSingleton<IDeliveryRequestRepository, DeliveryRequestRepository>();
            services.AddSingleton<OrderReadyForDeliveryEventHandler>();
            services.AddSingleton<GetDeliveryQueryHandler>();

            return services;
        }
    }
}