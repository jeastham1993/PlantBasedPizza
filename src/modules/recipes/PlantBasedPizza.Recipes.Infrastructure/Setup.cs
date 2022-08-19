using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddRecipeInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {            
            services.AddSingleton<IRecipeRepository, RecipeRepository>();

            return services;
        }
    }
}