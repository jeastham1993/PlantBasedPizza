namespace PlantBasedPizza.Recipes.Core.Entities
{
    public interface IRecipeRepository
    {
        Task<Recipe?> Retrieve(string recipeIdentifier);

        Task<IEnumerable<Recipe>> List();

        Task Add(Recipe? recipe);

        Task Update(Recipe? recipe);
    }
}