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