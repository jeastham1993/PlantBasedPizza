using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.Core.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Recipes.Core.Services;

public class RecipeFactory : IRecipeFactory
{
    private readonly IDomainEventDispatcher _eventDispatcher;

    public RecipeFactory(IDomainEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    public async Task<Recipe> CreateAsync(string recipeIdentifier, string name, decimal price, string correlationId = "")
    {
        var recipe = new Recipe(recipeIdentifier, name, price);

        await _eventDispatcher.PublishAsync(new RecipeCreatedEvent(recipe, correlationId));

        return recipe;
    }
}