using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantBasedPizza.Recipes.Core.Entities
{
    public interface IRecipeRepository
    {
        Task<Recipe> Retrieve(string recipeIdentifier);

        Task<IEnumerable<Recipe>> List();

        Task Add(Recipe recipe);

        Task Update(Recipe recipe);
    }
}