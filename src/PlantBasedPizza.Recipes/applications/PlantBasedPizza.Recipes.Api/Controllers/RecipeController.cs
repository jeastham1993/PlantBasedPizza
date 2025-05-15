using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Recipes.Core;
using PlantBasedPizza.Recipes.Core.CreateRecipe;

namespace PlantBasedPizza.Recipes.Api.Controllers
{
    [Route("recipes")]
    public class RecipeController(
        IRecipeRepository recipeRepository,
        CreateRecipeCommandHandler createRecipeCommandHandler)
        : ControllerBase
    {
        /// <summary>
        /// List all recipes.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IEnumerable<Recipe>> List()
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
        [Authorize(Roles = "admin,staff")]
        public async Task<RecipeDto?> Create([FromBody] CreateRecipeCommand request)
        {
            try
            {
                var recipe = await createRecipeCommandHandler.Handle(request);

                return recipe;
            }
            catch (ArgumentOutOfRangeException)
            {
                Response.StatusCode = 400;
                return null;
            }
        }
    }
}