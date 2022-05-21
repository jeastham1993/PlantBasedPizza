using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.Core.Events;
using PlantBasedPizza.Recipes.Core.Exceptions;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Recipes.Core.Commands;

public class CreateRecipeCommandHandler
{
    private readonly IObservabilityService _observability;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IEventBus _eventBus;

    public CreateRecipeCommandHandler(IObservabilityService observability, IRecipeRepository recipeRepository, IEventBus eventBus)
    {
        _observability = observability;
        _recipeRepository = recipeRepository;
        _eventBus = eventBus;
    }

    public async Task Handle(CreateRecipeCommand command)
    {
        this._observability.Info("Checking if recipe exists");

        var existingRecipe = await this._recipeRepository.Retrieve(command.RecipeIdentifier);

        if (existingRecipe != null)
        {
            this._observability.Info("Recipe exists, returning");

            await this._observability.PutMetric("Recipes", "DuplicateRecipeCreation", 1);

            throw new RecipeExistsException(existingRecipe);
        }

        var recipe = new Recipe(command.RecipeIdentifier, command.Name, command.Price);

        foreach (var item in command.Ingredients)
        {
            recipe.AddIngredient(item.Name, item.Quantity);
        }

        this._observability.Info("Creating recipe");

        await this._recipeRepository.Add(recipe);

        await this._eventBus.Publish(new RecipeCreatedEvent(recipe));
    }
}