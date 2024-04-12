using PlantBasedPizza.Api.Events;
using PlantBasedPizza.Events;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Core.Events
{
    public class RecipeCreatedEvent : IDomainEvent
    {
        public RecipeCreatedEvent(Recipe recipe, string correlationId)
        {
            this.Recipe = recipe;
            this.CorrelationId = correlationId;
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
        }
        
        public string EventName => "recipes.recipe-created";
        
        public string EventVersion => "v1";
        public string EventId { get; }
        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }
        public Recipe Recipe { get; }
    }
}