using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Recipes.Core.Commands;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure.Controllers
{
    [Route("recipes")]
    public class RecipeController(IRecipeRepository recipeRepository) : ControllerBase
    {
        /// <summary>
        /// List all recipes.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IEnumerable<Recipe?>> List()
        {
            return await recipeRepository.List();
        }

        /// <summary>
        /// Get a specific recipe.
        /// </summary>
        /// <param name="recipeIdentifier">The identifier of the recipe to get.</param>
        /// <returns></returns>
        [HttpGet("{recipeIdentifier}")]
        public async Task<Recipe?> Get(string recipeIdentifier)
        {
            return await recipeRepository.Retrieve(recipeIdentifier);
        }
        
        /// <summary>
        /// Create a new recipe.
        /// </summary>
        /// <param name="request">The <see cref="CreateRecipeCommand"/> request.</param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<Recipe?> Create([FromBody] CreateRecipeCommand request)
        {
            var existingRecipe = await recipeRepository.Retrieve(request.RecipeIdentifier);

            if (existingRecipe != null)
            {
                return existingRecipe;
            }

            var recipe = new Recipe(request.RecipeIdentifier, request.Name, request.Price);

            foreach (var item in request.Ingredients)
            {
                recipe.AddIngredient(item.Name, item.Quantity);
            }

            await recipeRepository.Add(recipe);

            return recipe;
        }
    }
}