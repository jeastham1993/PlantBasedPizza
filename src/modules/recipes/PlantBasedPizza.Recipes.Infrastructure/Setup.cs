using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.Core.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Recipes.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddRecipeInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {            
            services.AddSingleton<IRecipeRepository, RecipeRepository>();

            services.AddSingleton<Handles<RecipeCreatedEvent>, IntegrationEventPublisher>();

            return services;
        }
    }
}