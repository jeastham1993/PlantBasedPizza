using System.Text.Json.Serialization;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Recipes.Core.IntegrationEvents;

public class RecipeCreatedEventV1 : IntegrationEvent
{
    public override string EventName => "recipe.recipeCreated";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://recipes.plantbasedpizza");
    
    [JsonPropertyName("RecipeIdentifier")]
    public string RecipeIdentifier { get; init; } = "";
}