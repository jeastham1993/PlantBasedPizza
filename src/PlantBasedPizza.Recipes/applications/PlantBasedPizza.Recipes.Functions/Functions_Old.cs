/*using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using PlantBasedPizza.Recipes.Core;
using PlantBasedPizza.Recipes.Core.CreateRecipe;

namespace PlantBasedPizza.Recipes.Functions;

public class Functions(IRecipeRepository recipeRepository)
{
    [Function("ListRecipes")]
    public async Task<IActionResult> HandleListRecipes([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "recipes")] HttpRequest req)
    {
        var recipes = await recipeRepository.List();
        
        return new OkObjectResult(recipes);
    }
    
    [Function("GetRecipe")]
    public async Task<IActionResult> HandleGetRecipe([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "recipes/{recipeId}")] HttpRequest req)
    {
        var recipeId = req.RouteValues["recipeId"];

        if (recipeId == null)
        {
            return new BadRequestResult();
        }
        
        var recipe = await recipeRepository.Retrieve(recipeId.ToString());
        
        return recipe is null ? new NotFoundResult() : new OkObjectResult(recipe);
    }
    
    [Function("CreateRecipe")]
    public async Task<IActionResult> HandleCreateRecipe([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "recipes")] HttpRequest req)
    {
        try
        {
            var request = await req.ReadFromJsonAsync<CreateRecipeCommand>();
            
            var existingRecipe = await recipeRepository.Retrieve(request.RecipeIdentifier);

            if (existingRecipe != null)
            {
                return new OkObjectResult(existingRecipe);
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

            await recipeRepository.Add(recipe);

            return new OkObjectResult(recipe);
        }
        catch (ArgumentOutOfRangeException)
        {
            return new BadRequestResult();
        }
    }
}*/