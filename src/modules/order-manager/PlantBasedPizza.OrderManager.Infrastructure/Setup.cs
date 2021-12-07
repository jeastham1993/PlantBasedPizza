using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Core.Handlers;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddOrderManagerInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            BsonClassMap.RegisterClassMap<Order>(map =>
            {
                map.AutoMap();
                map.MapField("_items");
                map.MapField("_history");
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            BsonClassMap.RegisterClassMap<OrderItem>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            BsonClassMap.RegisterClassMap<DeliveryDetails>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            // or use a connection string
            var client = new MongoClient(configuration["DatabaseConnection"]);

            services.AddSingleton<MongoClient>(client);
            
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<IRecipeService, RecipeService>();
            services.AddTransient<Handles<OrderPreparingEvent>, OrderPreparingEventHandler>();
            services.AddTransient<Handles<OrderPrepCompleteEvent>, OrderPrepCompleteEventHandler>();
            services.AddTransient<Handles<OrderBakedEvent>, OrderBakedEventHandler>();
            services.AddTransient<Handles<OrderQualityCheckedEvent>, OrderQualityCheckedEventHandler>();
            services.AddTransient<Handles<OrderDeliveredEvent>, DriverDeliveredOrderEventHandler>();
            services.AddTransient<Handles<DriverCollectedOrderEvent>, DriverCollectedOrderEventHandler>();

            return services;
        }
    }
}