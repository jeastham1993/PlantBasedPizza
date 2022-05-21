using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.Core.Events;
using PlantBasedPizza.Recipes.Core.Exceptions;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Recipes.Core.Commands;

public class UpdateRecipeCommandHandler
{
    private readonly IObservabilityService _observability;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IEventBus _eventBus;

    public UpdateRecipeCommandHandler(IObservabilityService observability, IRecipeRepository recipeRepository, IEventBus eventBus)
    {
        _observability = observability;
        _recipeRepository = recipeRepository;
        _eventBus = eventBus;
    }

    public async Task Handle(UpdateRecipeCommand command)
    {
        this._observability.Info("Checking if recipe exists");

        var existingRecipe = await this._recipeRepository.Retrieve(command.RecipeIdentifier);

        if (existingRecipe == null)
        {
            this._observability.Info("Recipe not found");

            await this._observability.PutMetric("Recipes", "RecipeNotFound", 1);

            throw new RecipeNotFoundException(command.RecipeIdentifier);
        }

        var updateIngredients = new List<Ingredient>();

        foreach (var ingredient in command.Ingredients)
        {
            updateIngredients.Add(new Ingredient(ingredient.Name, ingredient.Quantity));
        }

        existingRecipe.UpdateFrom(command.Name, command.Price, updateIngredients);

        this._observability.Info("Updating recipe");

        await this._recipeRepository.Update(existingRecipe);

        await this._eventBus.Publish(new RecipeUpdatedEvent(existingRecipe));
    }
}