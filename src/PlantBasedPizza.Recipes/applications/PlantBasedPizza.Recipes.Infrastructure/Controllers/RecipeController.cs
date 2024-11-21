using Dapr.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Recipes.Core.Commands;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.Core.IntegrationEvents;

namespace PlantBasedPizza.Recipes.Infrastructure.Controllers
{
    [Route("recipes")]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly DaprClient _daprClient;

        public RecipeController(IRecipeRepository recipeRepository,  DaprClient daprClient)
        {
            _recipeRepository = recipeRepository;
            _daprClient = daprClient;
        }

        /// <summary>
        /// List all recipes.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IEnumerable<Recipe>> List()
        {
            return await _recipeRepository.List();
        }

        /// <summary>
        /// Get a specific recipe.
        /// </summary>
        /// <param name="recipeIdentifier">The identifier of the recipe to get.</param>
        /// <returns></returns>
        [HttpGet("{recipeIdentifier}")]
        public async Task<Recipe> Get(string recipeIdentifier)
        {
            return await _recipeRepository.Retrieve(recipeIdentifier);
        }
        
        /// <summary>
        /// Create a new recipe.
        /// </summary>
        /// <param name="request">The <see cref="CreateRecipeCommand"/> request.</param>
        /// <returns></returns>
        [HttpPost("")]
        [Authorize(Roles = "admin,staff")]
        public async Task<Recipe?> Create([FromBody] CreateRecipeCommand request)
        {
            try
            {
                var existingRecipe = await _recipeRepository.Retrieve(request.RecipeIdentifier);

                if (existingRecipe != null)
                {
                    return existingRecipe;
                }

                var category = request.Category switch
                {
                    "pizza" => RecipeCategory.Pizza,
                    "sides" => RecipeCategory.Sides,
                    "drinks" => RecipeCategory.Drinks,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var recipe = new Recipe(category, request.RecipeIdentifier, request.Name, request.Price);

                foreach (var item in request.Ingredients)
                {
                    recipe.AddIngredient(item.Name, item.Quantity);
                }

                await _recipeRepository.Add(recipe);
                var evt = new RecipeCreatedEventV1()
                {
                    RecipeIdentifier = recipe.RecipeIdentifier
                };
                await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);

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