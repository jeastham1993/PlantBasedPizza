using System;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Recipes.Core.Events
{
    public class RecipeUpdatedEvent : BaseEvent, IDomainEvent
    {
        public RecipeUpdatedEvent(Recipe recipe) : base()
        {
            this.Recipe = recipe;
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
        }
        
        public string EventName => "recipes.recipe-updated";
        public string EventId { get; }
        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }
        public Recipe Recipe { get; }
    }
}