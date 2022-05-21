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
using PlantBasedPizza.Recipes.Core.Exceptions;

namespace PlantBasedPizza.Recipes.Serverless
{
    public class CreateRecipe
    {
        private readonly IObservabilityService _observability;
        private readonly CreateRecipeCommandHandler _commandHandler;

        public CreateRecipe()
        {
            Startup.Configure();

            this._observability = Startup.Services.GetRequiredService<IObservabilityService>();
            this._commandHandler = Startup.Services.GetRequiredService<CreateRecipeCommandHandler>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apiRequest, ILambdaContext context)
        {
            this._observability.AddCorrelationContext(apiRequest.Headers);

            this._observability.Info("Received request to create a recipe");

            return await this._observability.TraceMethodAsync("Create Recipe",
                async () =>
                {
                    var request = JsonSerializer.Deserialize<CreateRecipeCommand>(apiRequest.Body);

                    await this._commandHandler.Handle(request);
                    
                    try
                    {
                        return new APIGatewayProxyResponse()
                        {
                            StatusCode = (int)HttpStatusCode.Created,
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
                    catch (RecipeExistsException e)
                    {
                        return new APIGatewayProxyResponse()
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = JsonSerializer.Serialize(e.ExistingRecipe),
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
                });
        }
    }
}
