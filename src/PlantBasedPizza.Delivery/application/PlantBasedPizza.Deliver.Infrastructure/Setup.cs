using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PlantBasedPizza.Deliver.Core.AssignDriver;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.GetAwaitingCollection;
using PlantBasedPizza.Deliver.Core.GetDeliveryStatus;
using PlantBasedPizza.Deliver.Core.MarkOrderDelivered;
using PlantBasedPizza.Deliver.Core.OrderReadyForDelivery;
using PlantBasedPizza.Shared.Caching;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    using MongoDB.Bson.Serialization;

    public static class Setup
    {
        public static IServiceCollection AddDeliveryInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            var client = new MongoClient(configuration["DatabaseConnection"]);

            services.AddSingleton(client);
            services.AddDaprClient();
            services.AddCaching(configuration);

            BsonClassMap.RegisterClassMap<OutboxItem>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
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
            services.AddSingleton<IDeadLetterRepository, DeadLetterRepository>();
            services.AddSingleton<OrderReadyForDeliveryEventHandler>();
            services.AddSingleton<AssignDriverRequestHandler>();
            services.AddSingleton<MarkOrderDeliveredRequestHandler>();
            services.AddSingleton<GetDeliveryQueryHandler>();
            services.AddSingleton<GetAwaitingCollectionQueryHandler>();
            
            return services;
        }
    }
}