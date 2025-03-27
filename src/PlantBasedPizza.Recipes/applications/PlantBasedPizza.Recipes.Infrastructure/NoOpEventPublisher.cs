using PlantBasedPizza.Recipes.Core.IntegrationEvents;

namespace PlantBasedPizza.Recipes.Infrastructure;

public class NoOpEventPublisher : IEventPublisher
{
    public Task Publish(RecipeCreatedEventV1 evt)
    {
        // No-op
        return Task.CompletedTask;
    }
}