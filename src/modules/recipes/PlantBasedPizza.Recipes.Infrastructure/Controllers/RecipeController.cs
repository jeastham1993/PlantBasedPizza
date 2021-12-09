using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        [HttpGet("recipes")]
        public async Task<IEnumerable<Recipe>> List()
        {
            return await this._observability.TraceMethodAsync("List Recipes",
                async () => await this._recipeRepository.List());
        }

        [HttpGet("recipes/{recipeIdentifier}")]
        public async Task<Recipe> Get(string recipeIdentifier)
        {
            return await this._observability.TraceMethodAsync("Get Recipes",
                async () => await this._recipeRepository.Retrieve(recipeIdentifier));
        }

        [HttpPost("recipes")]
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