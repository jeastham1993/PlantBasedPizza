using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.DataTransfer;

namespace PlantBasedPizza.Recipes.Infrastructure;

public static class Setup
{
    public static IServiceCollection AddRecipeInfrastructure(this IServiceCollection services,
        IConfiguration configuration, Serilog.ILogger logger, string? overrideConnectionString = null)
    {
        // Register DbContext
        services.AddDbContext<RecipesDbContext>(options =>
            options.UseNpgsql(
                    overrideConnectionString ?? configuration.GetConnectionString("RecipesPostgresConnection"),
                    b =>
                        b.MigrationsAssembly("PlantBasedPizza.Recipes.Infrastructure")
                            .EnableRetryOnFailure(
                                2,
                                TimeSpan.FromSeconds(2),
                                null))
                .LogTo(logger.Information));

        services.AddTransient<RecipeDataTransferService>();
        services.AddScoped<IRecipeRepository, RecipeRepositoryPostgres>();
        
        // Domain services and factories
        services.AddTransient<PlantBasedPizza.Recipes.Core.Services.IRecipeFactory, PlantBasedPizza.Recipes.Core.Services.RecipeFactory>();
        services.AddTransient<PlantBasedPizza.Recipes.Core.Services.IRecipeDomainService, PlantBasedPizza.Recipes.Core.Services.RecipeDomainService>();

        return services;
    }
}