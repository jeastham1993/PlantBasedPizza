using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Handlers;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    using MongoDB.Bson.Serialization;

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
            
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddSingleton<CollectOrderCommandHandler>();
            services.AddSingleton<AddItemToOrderHandler>();
            services.AddSingleton<CreateDeliveryOrderCommandHandler>();
            services.AddSingleton<CreatePickupOrderCommandHandler>();
            services.AddSingleton<IRecipeService, RecipeService>();
            services.AddSingleton<ILoyaltyPointService, LoyaltyPointService>();
            
            services.AddSingleton<Handles<OrderPreparingEvent>, OrderPreparingEventHandler>();
            services.AddSingleton<Handles<OrderPrepCompleteEvent>, OrderPrepCompleteEventHandler>();
            services.AddSingleton<Handles<OrderBakedEvent>, OrderBakedEventHandler>();
            services.AddSingleton<Handles<OrderQualityCheckedEvent>, OrderQualityCheckedEventHandler>();
            services.AddSingleton<Handles<OrderDeliveredEvent>, DriverDeliveredOrderEventHandler>();
            services.AddSingleton<Handles<DriverCollectedOrderEvent>, DriverCollectedOrderEventHandler>();

            return services;
        }
    }
}