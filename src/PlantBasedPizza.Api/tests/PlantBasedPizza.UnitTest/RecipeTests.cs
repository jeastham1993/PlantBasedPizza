using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.Core.Events;
using PlantBasedPizza.Recipes.Core.Services;
using PlantBasedPizza.Shared.Events;
using Xunit;

namespace PlantBasedPizza.UnitTest;

public class RecipeTests
{
    internal const string DefaultRecipeIdentifier = "MyRecipe";
    
    [Fact]
    public async Task CanCreateNewRecipe_ShouldSetDefaultFields()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var recipeFactory = new RecipeFactory(mockEventDispatcher);

        // Act
        var recipe = await recipeFactory.CreateAsync(DefaultRecipeIdentifier, "Pizza", 6.5M);
        
        recipe.AddIngredient("Base", 1);
        recipe.AddIngredient("Tomato Sauce", 1);
        recipe.AddIngredient("Cheese", 1);

        // Assert
        recipe.RecipeIdentifier.Should().Be(DefaultRecipeIdentifier);
        recipe.Name.Should().Be("Pizza");
        recipe.Price.Should().Be(6.5M);
        recipe.Ingredients.Count.Should().Be(3);

        A.CallTo(() => mockEventDispatcher.PublishAsync(A<RecipeCreatedEvent>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}