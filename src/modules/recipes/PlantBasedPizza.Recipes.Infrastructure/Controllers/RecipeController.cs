using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Recipes.Core.Commands;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure.Controllers
{
    [Route("recipes")]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeRepository _recipeRepository;

        public RecipeController(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        [HttpGet("recipes")]
        public async Task<IEnumerable<Recipe>> List()
        {
            return await this._recipeRepository.List().ConfigureAwait(false);
        }

        [HttpGet("recipes/{recipeIdentifier}")]
        public async Task<Recipe> Get(string recipeIdentifier)
        {
            return await this._recipeRepository.Retrieve(recipeIdentifier).ConfigureAwait(false);
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