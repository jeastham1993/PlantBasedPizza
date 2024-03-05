using FluentAssertions;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.Core.Events;
using PlantBasedPizza.Shared.Events;
using Xunit;

namespace PlantBasedPizza.UnitTest;

public class RecipeTests
{
    internal const string DefaultRecipeIdentifier = "MyRecipe";
    
    [Fact]
    public void CanCreateNewOrder_ShouldSetDefaultFields()
    {
        Recipe? createdRecipe = null;
        
        DomainEvents.Register<RecipeCreatedEvent>((evt) =>
        {
            createdRecipe = evt.Recipe;
        });

        var recipe = new Recipe(DefaultRecipeIdentifier, "Pizza", 6.5M);
        
        recipe.AddIngredient("Base", 1);
        recipe.AddIngredient("Tomato Sauce", 1);
        recipe.AddIngredient("Cheese", 1);

        recipe.RecipeIdentifier.Should().Be(DefaultRecipeIdentifier);
        recipe.Name.Should().Be("Pizza");
        recipe.Price.Should().Be(6.5M);
        recipe.Ingredients.Count.Should().Be(3);

        createdRecipe.Should().NotBeNull();
    }
}