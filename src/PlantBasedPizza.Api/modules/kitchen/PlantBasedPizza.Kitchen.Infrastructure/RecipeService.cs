using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.DataTransfer;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public class RecipeService : IRecipeService
    {
        private readonly RecipeDataTransferService _recipes;

        public RecipeService(RecipeDataTransferService recipes)
        {
            _recipes = recipes;
        }

        public async Task<RecipeAdapter> GetRecipe(string recipeIdentifier)
        {
            var recipe = await this._recipes.GetRecipeAsync(recipeIdentifier);

            return new RecipeAdapter(recipe.RecipeId);
        }
    }
}