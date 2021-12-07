using System;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Recipes.Core.Events
{
    public class RecipeCreatedEvent : IDomainEvent
    {
        public RecipeCreatedEvent(Recipe recipe)
        {
            this.Recipe = recipe;
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
        }
        
        public string EventName => "recipes.recipe-created";
        public string EventId { get; }
        public DateTime EventDate { get; }
        public Recipe Recipe { get; }
    }
}