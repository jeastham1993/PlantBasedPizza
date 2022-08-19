using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddSingleton<IRecipeService, RecipeService>();
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