using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Handlers;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddKitchenInfrastructure(this IServiceCollection services,
            IConfiguration configuration, string? overrideConnectionString = null)
        {
            // Register DbContext
            services.AddDbContext<KitchenDbContext>(options =>
                options.UseNpgsql(
                    overrideConnectionString ?? configuration.GetConnectionString("KitchenPostgresConnection"),
                    b => b.MigrationsAssembly("PlantBasedPizza.Kitchen.Infrastructure")));

            services.AddTransient<IRecipeService, RecipeService>();
            services.AddTransient<IOrderManagerService, OrderManagerService>();
            services.AddTransient<Handles<OrderSubmittedEvent>, OrderSubmittedEventHandler>();
            services.AddScoped<IKitchenRequestRepository, KitchenRequestRepositoryPostgres>();

            return services;
        }
    }
}