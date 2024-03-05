using System.Collections.Generic;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.UnitTest.Builders
{
    public class KitchenRequestBuilder
    {
        private readonly KitchenRequest? _request = null;
        internal const string OrderIdentifier = "ORDER123";

        public KitchenRequestBuilder()
        {
            this._request = new KitchenRequest(OrderIdentifier, new List<RecipeAdapter>(1));
        }

        public KitchenRequestBuilder AddRecipe(string recipeName)
        {
            if (this._request == null)
            {
                return this;
            }
            
            this._request.Recipes.Add(new RecipeAdapter(recipeName.ToUpper()));

            return this;
        }

        public KitchenRequest? Build()
        {
            return this._request;
        }
    }
}