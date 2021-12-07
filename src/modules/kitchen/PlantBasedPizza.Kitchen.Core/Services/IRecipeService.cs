using System.Threading.Tasks;
using PlantBasedPizza.Kitchen.Core.Adapters;

namespace PlantBasedPizza.Kitchen.Core.Services
{
    public interface IRecipeService
    {
        Task<RecipeAdapter> GetRecipe(string recipeIdentifier);
    }
}