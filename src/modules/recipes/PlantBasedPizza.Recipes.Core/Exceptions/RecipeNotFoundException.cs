using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Core.Exceptions;

public class RecipeNotFoundException : Exception
{
    public RecipeNotFoundException(string recipeIdentifier) : base()
    {
        this.RecipeIdentifier = recipeIdentifier;
    }
    
    public string RecipeIdentifier { get; private set; }
}