namespace PlantBasedPizza.Recipes.Core.IntegrationEvents;

public interface IEventPublisher
{
    Task Publish(RecipeCreatedEventV1 evt);
}