using FluentAssertions;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.UnitTests;

public class RecipeTests
{
    internal const string DefaultRecipeIdentifier = "MyRecipe";
    
    [Fact]
    public void CanCreateNewOrder_ShouldSetDefaultFields()
    {
        var recipe = new Recipe(RecipeCategory.Pizza, DefaultRecipeIdentifier, "Pizza", 6.5M);
        
        recipe.AddIngredient("Base", 1);
        recipe.AddIngredient("Tomato Sauce", 1);
        recipe.AddIngredient("Cheese", 1);

        recipe.RecipeIdentifier.Should().Be(DefaultRecipeIdentifier);
        recipe.Name.Should().Be("Pizza");
        recipe.Price.Should().Be(6.5M);
        recipe.Ingredients.Count.Should().Be(3);
    }
}