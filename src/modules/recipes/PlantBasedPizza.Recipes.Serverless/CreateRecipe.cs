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
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Recipes.Core.Events;

namespace PlantBasedPizza.Recipes.Serverless
{
    public class CreateRecipe
    {
        private readonly IObservabilityService _observability;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IEventBus _eventBus;

        public CreateRecipe()
        {
            Startup.Configure();

            this._observability = Startup.Services.GetRequiredService<IObservabilityService>();
            this._recipeRepository = Startup.Services.GetRequiredService<IRecipeRepository>();
            this._eventBus = Startup.Services.GetRequiredService<IEventBus>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apiRequest, ILambdaContext context)
        {
            this._observability.AddCorrelationContext(apiRequest.Headers);

            this._observability.Info("Received request to create a recipce");

            return await this._observability.TraceMethodAsync("Get Recipes",
                async () =>
                {
                    var request = JsonSerializer.Deserialize<CreateRecipeCommand>(apiRequest.Body);

                    this._observability.Info("Checking if recipe exists");

                    var existingRecipe = await this._recipeRepository.Retrieve(request.RecipeIdentifier);

                    if (existingRecipe != null)
                    {
                        this._observability.Info("Recipe exists, returning");

                        await this._observability.PutMetric("Recipes", "DuplicateRecipeCreation", 1);

                        return new APIGatewayProxyResponse()
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = JsonSerializer.Serialize(existingRecipe),
                            Headers = new Dictionary<string, string>()
                        {
                            {
                                "Content-Type", "application/json"
                            },
                            {
                                CorrelationContext.DefaultRequestHeaderName, CorrelationContext.GetCorrelationId()
                            }
                        }
                        };
                    }

                    var recipe = new Recipe(request.RecipeIdentifier, request.Name, request.Price);

                    foreach (var item in request.Ingredients)
                    {
                        recipe.AddIngredient(item.Name, item.Quantity);
                    }

                    this._observability.Info("Creating recipe");

                    await this._recipeRepository.Add(recipe);

                    await this._eventBus.Publish(new RecipeCreatedEvent(recipe));

                    return new APIGatewayProxyResponse()
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = JsonSerializer.Serialize(recipe),
                        Headers = new Dictionary<string, string>()
                        {
                            {
                                "Content-Type", "application/json"
                            },
                            {
                                CorrelationContext.DefaultRequestHeaderName, CorrelationContext.GetCorrelationId()
                            }
                        }
                    };
                });
        }
    }
}
