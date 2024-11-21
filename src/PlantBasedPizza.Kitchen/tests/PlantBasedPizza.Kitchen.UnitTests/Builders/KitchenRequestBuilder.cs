using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.UnitTest.Builders
{
    public class KitchenRequestBuilder
    {
        private readonly KitchenRequest? _request = null;
        internal const string OrderIdentifier = "ORDER123";

        public KitchenRequestBuilder()
        {
            _request = new KitchenRequest(OrderIdentifier, new List<RecipeAdapter>(1));
        }

        public KitchenRequestBuilder AddRecipe(string recipeName)
        {
            if (_request == null)
            {
                return this;
            }
            
            _request.Recipes.Add(new RecipeAdapter(recipeName.ToUpper()));

            return this;
        }

        public KitchenRequest? Build()
        {
            return _request;
        }
    }
}