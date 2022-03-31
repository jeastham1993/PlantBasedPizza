using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Core.Exceptions;

public class RecipeExistsException : Exception
{
    public RecipeExistsException(Recipe recipe) : base()
    {
        this.ExistingRecipe = recipe;
    }
    
    public Recipe ExistingRecipe { get; private set; }
}