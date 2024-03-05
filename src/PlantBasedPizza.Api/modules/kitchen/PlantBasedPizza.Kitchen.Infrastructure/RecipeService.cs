using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipes;

        public RecipeService(IRecipeRepository recipes)
        {
            _recipes = recipes;
        }

        public async Task<RecipeAdapter> GetRecipe(string recipeIdentifier)
        {
            var recipe = await this._recipes.Retrieve(recipeIdentifier);

            return new RecipeAdapter(recipe.RecipeIdentifier);
        }
    }
}