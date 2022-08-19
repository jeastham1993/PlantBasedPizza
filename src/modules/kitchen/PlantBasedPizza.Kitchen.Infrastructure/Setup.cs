using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Handlers;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddKitchenInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IRecipeService, RecipeService>();
            services.AddSingleton<IOrderManagerService, OrderManagerService>();
            services.AddSingleton<Handles<OrderSubmittedEvent>, OrderSubmittedEventHandler>();
            services.AddSingleton<IKitchenRequestRepository, KitchenRequestRepository>();

            return services;
        }
    }
}