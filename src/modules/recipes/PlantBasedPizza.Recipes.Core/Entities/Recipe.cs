using System.Collections.Generic;
using PlantBasedPizza.Recipes.Core.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Recipes.Core.Entities
{
    public class Recipe
    {
        private List<Ingredient> _ingredients;

        internal Recipe()
        {
        }
        
        public Recipe(string recipeIdentifier, string name, decimal price)
        {
            this.RecipeIdentifier = recipeIdentifier;
            this.Name = name;
            this.Price = price;

            DomainEvents.Raise(new RecipeCreatedEvent(this));
        }
        
        public string RecipeIdentifier { get; private set; }
        public string Name { get; private set; }
        public decimal Price { get; private set; }

        public IReadOnlyCollection<Ingredient> Ingredients => this._ingredients;

        public void AddIngredient(string name, int quantity)
        {
            if (this._ingredients == null)
            {
                this._ingredients = new List<Ingredient>();
            }
            
            this._ingredients.Add(new Ingredient(name, quantity));
        }
    }
}