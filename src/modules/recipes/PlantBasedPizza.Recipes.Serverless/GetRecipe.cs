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

namespace PlantBasedPizza.Recipes.Serverless
{
    public class GetRecipe
    {
        private readonly IObservabilityService _observability;
        private readonly IRecipeRepository _recipeRepository;

        public GetRecipe()
        {
            Startup.Configure();

            this._observability = Startup.Services.GetRequiredService<IObservabilityService>();
            this._recipeRepository = Startup.Services.GetRequiredService<IRecipeRepository>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await this._observability.TraceMethodAsync("Get Recipes",
                async () =>
                {
                    var recipe = await this._recipeRepository.Retrieve(request.PathParameters["recipeIdentifier"]);

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
