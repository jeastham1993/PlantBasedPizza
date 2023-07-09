using PlantBasedPizza.Recipes.Core.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Recipes.Infrastructure;

public class IntegrationEventPublisher : Handles<RecipeCreatedEvent>
{
    private readonly IEventBus _eventBus;

    public IntegrationEventPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(RecipeCreatedEvent evt) => await this._eventBus.Publish(evt);
}