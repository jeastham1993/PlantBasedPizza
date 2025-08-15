using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Core.Services;

public interface IRecipeDomainService
{
    Task UpdateRecipeAsync(Recipe recipe, string correlationId = "");
    Task DeleteRecipeAsync(Recipe recipe, string correlationId = "");
}