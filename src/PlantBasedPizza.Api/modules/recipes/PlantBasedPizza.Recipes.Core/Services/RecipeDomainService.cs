using PlantBasedPizza.Events;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Recipes.Core.Services;

public class RecipeDomainService : IRecipeDomainService
{
    private readonly IDomainEventDispatcher _eventDispatcher;

    public RecipeDomainService(IDomainEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    public Task UpdateRecipeAsync(Recipe recipe, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(recipe);

        // Update business logic would go here
        // No specific event needed for this operation in current domain
        return Task.CompletedTask;
    }

    public Task DeleteRecipeAsync(Recipe recipe, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(recipe);

        // Delete business logic would go here
        // No specific event needed for this operation in current domain
        return Task.CompletedTask;
    }
}