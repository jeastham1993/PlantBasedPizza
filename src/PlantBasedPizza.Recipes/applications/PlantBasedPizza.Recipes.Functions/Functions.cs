// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Azure.Functions.Worker;
// using PlantBasedPizza.Recipes.Core;
// using PlantBasedPizza.Recipes.Core.CreateRecipe;
//
// namespace PlantBasedPizza.Recipes.Functions;
//
// public class Functions(IRecipeRepository recipeRepository, CreateRecipeCommandHandler createRecipeCommandHandler)
// {
//     [Function("ListRecipes")]
//     public async Task<IActionResult> HandleListRecipes([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "recipes")] HttpRequest req)
//     {
//         var recipes = await recipeRepository.List();
//         
//         return new OkObjectResult(recipes);
//     }
//     
//     [Function("GetRecipe")]
//     public async Task<IActionResult> HandleGetRecipe([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "recipes/{recipeId}")] HttpRequest req)
//     {
//         var recipeId = req.RouteValues["recipeId"];
//
//         if (recipeId == null)
//         {
//             return new BadRequestResult();
//         }
//         
//         var recipe = await recipeRepository.Retrieve(recipeId.ToString());
//         
//         return recipe is null ? new NotFoundResult() : new OkObjectResult(recipe);
//     }
//     
//     [Function("CreateRecipe")]
//     public async Task<IActionResult> HandleCreateRecipe([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "recipes")] HttpRequest req)
//     {
//         try
//         {
//             var command = await req.ReadFromJsonAsync<CreateRecipeCommand>();
//             
//             var recipe = await createRecipeCommandHandler.Handle(command);
//
//             return new OkObjectResult(recipe);
//         }
//         catch (ArgumentOutOfRangeException)
//         {
//             return new BadRequestResult();
//         }
//     }
// }