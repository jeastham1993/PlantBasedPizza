using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure
{
    using MongoDB.Bson.Serialization;

    public static class Setup
    {
        public static IServiceCollection AddRecipeInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {         
            var client = new MongoClient(configuration["DatabaseConnection"]);

            services.AddSingleton(client);
            services.AddDaprClient();
            
            BsonClassMap.RegisterClassMap<Recipe>(map =>
            {
                map.AutoMap();
                map.MapField("_ingredients");
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            BsonClassMap.RegisterClassMap<Ingredient>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            services.AddSingleton<IRecipeRepository, RecipeRepository>();

            return services;
        }
    }
}