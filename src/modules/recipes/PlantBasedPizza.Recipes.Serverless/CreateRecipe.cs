using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Net;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Shared.Logging;
using PlantBasedPizza.Recipes.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using PlantBasedPizza.Recipes.Core.Commands;

namespace PlantBasedPizza.Recipes.Serverless
{
    public class CreateRecipe
    {
        private readonly IObservabilityService _observability;
        private readonly IRecipeRepository _recipeRepository;

        public CreateRecipe()
        {
            Startup.Configure();

            this._observability = Startup.Services.GetRequiredService<IObservabilityService>();
            this._recipeRepository = Startup.Services.GetRequiredService<IRecipeRepository>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apiRequest, ILambdaContext context)
        {
            return await this._observability.TraceMethodAsync("Get Recipes",
                async () =>
                {
                    var request = JsonSerializer.Deserialize<CreateRecipeCommand>(apiRequest.Body);

                    var existingRecipe = await this._recipeRepository.Retrieve(request.RecipeIdentifier);

                    if (existingRecipe != null)
                    {
                        return new APIGatewayProxyResponse()
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = JsonSerializer.Serialize(existingRecipe),
                            Headers = new Dictionary<string, string>()
                        {
                            {
                                "Content-Type", "application/json"
                            }
                        }
                        };
                    }

                    var recipe = new Recipe(request.RecipeIdentifier, request.Name, request.Price);

                    foreach (var item in request.Ingredients)
                    {
                        recipe.AddIngredient(item.Name, item.Quantity);
                    }

                    await this._recipeRepository.Add(recipe);

                    return new APIGatewayProxyResponse()
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = JsonSerializer.Serialize(recipe),
                        Headers = new Dictionary<string, string>()
                        {
                            {
                                "Content-Type", "application/json"
                            }
                        }
                    };
                });
        }
    }
}
