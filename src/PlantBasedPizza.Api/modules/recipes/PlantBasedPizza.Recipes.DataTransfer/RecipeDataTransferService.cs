using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.DataTransfer;

public class RecipeDataTransferService(IRecipeRepository recipeRepository)
{
    public async Task<RecipeDTO> GetRecipeAsync(string recipeId)
    {
        var recipe = await recipeRepository.Retrieve(recipeId);

        return new RecipeDTO(recipeId, recipe.Name, recipe.Price);
    }
}