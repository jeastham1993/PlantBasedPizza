using Amazon;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddSingleton(new AmazonDynamoDBClient());
            
            services.AddSingleton<IDeliveryRequestRepository, DeliveryRequestRepository>();
            services.AddSingleton<Handles<OrderReadyForDeliveryEvent>, OrderReadyForDeliveryEventHandler>();

            return services;
        }
    }
}