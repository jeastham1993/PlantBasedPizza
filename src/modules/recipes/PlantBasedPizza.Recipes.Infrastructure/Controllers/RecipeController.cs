using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Recipes.Core.Commands;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Recipes.Infrastructure.Controllers
{
    [Route("recipes")]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IObservabilityService _observability;

        public RecipeController(IRecipeRepository recipeRepository, IObservabilityService observability)
        {
            _recipeRepository = recipeRepository;
            _observability = observability;
        }

        /// <summary>
        /// List all recipes.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IEnumerable<Recipe>> List()
        {
            this._observability.Info("Retrieved request to list recipes");

            return await this._recipeRepository.List();
        }

        /// <summary>
        /// Get a specific recipe.
        /// </summary>
        /// <param name="recipeIdentifier">The identifier of the recipe to get.</param>
        /// <returns></returns>
        [HttpGet("{recipeIdentifier}")]
        public async Task<Recipe> Get(string recipeIdentifier)
        {
            return await this._recipeRepository.Retrieve(recipeIdentifier);
        }
        
        /// <summary>
        /// Create a new recipe.
        /// </summary>
        /// <param name="request">The <see cref="CreateRecipeCommand"/> request.</param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<Recipe> Create([FromBody] CreateRecipeCommand request)
        {
            var existingRecipe = await this._recipeRepository.Retrieve(request.RecipeIdentifier);

            if (existingRecipe != null)
            {
                return existingRecipe;
            }

            var recipe = new Recipe(request.RecipeIdentifier, request.Name, request.Price);

            foreach (var item in request.Ingredients)
            {
                recipe.AddIngredient(item.Name, item.Quantity);
            }

            await this._recipeRepository.Add(recipe);

            return recipe;
        }
    }
}