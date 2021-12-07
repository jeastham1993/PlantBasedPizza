using System.Threading.Tasks;
using PlantBasedPizza.OrderManager.Core.ViewModels;

namespace PlantBasedPizza.OrderManager.Core.Services
{
    public interface IRecipeService
    {
        Task<Recipe> GetRecipe(string recipeIdentifier);
    }
}