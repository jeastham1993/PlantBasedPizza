using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Core.Services;

public interface IRecipeFactory
{
    Task<Recipe> CreateAsync(string recipeIdentifier, string name, decimal price, string correlationId = "");
}