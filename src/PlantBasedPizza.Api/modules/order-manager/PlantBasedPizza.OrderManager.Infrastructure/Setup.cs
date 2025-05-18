using Microsoft.EntityFrameworkCore;
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
using PlantBasedPizza.OrderManager.DataTransfer;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddOrderManagerInfrastructure(this IServiceCollection services,
            IConfiguration configuration, string? overrideConnectionString = null)

        {
            // Register DbContext
            services.AddDbContext<OrderManagerDbContext>(options =>
                options.UseNpgsql(
                    overrideConnectionString ?? configuration.GetConnectionString("OrderManagerPostgresConnection"),
                    b => b.MigrationsAssembly("PlantBasedPizza.OrderManager.Infrastructure")));

            services.AddTransient<OrderManagerDataTransferService>();
            services.AddScoped<IOrderRepository, OrderRepositoryPostgres>();
            services.AddTransient<CollectOrderCommandHandler>();
            services.AddTransient<AddItemToOrderHandler>();
            services.AddTransient<CreateDeliveryOrderCommandHandler>();
            services.AddTransient<CreatePickupOrderCommandHandler>();
            services.AddTransient<IRecipeService, RecipeService>();
            services.AddTransient<OrderEventPublisher, DaprEventPublisher>();

            services.AddTransient<Handles<OrderPreparingEvent>, OrderPreparingEventHandler>();
            services.AddTransient<Handles<OrderPrepCompleteEvent>, OrderPrepCompleteEventHandler>();
            services.AddTransient<Handles<OrderBakedEvent>, OrderBakedEventHandler>();
            services.AddTransient<Handles<OrderQualityCheckedEvent>, OrderQualityCheckedEventHandler>();
            services.AddTransient<Handles<OrderDeliveredEvent>, DriverDeliveredOrderEventHandler>();
            services.AddTransient<Handles<DriverCollectedOrderEvent>, DriverCollectedOrderEventHandler>();

            services.AddDaprClient();

            return services;
        }
    }
}