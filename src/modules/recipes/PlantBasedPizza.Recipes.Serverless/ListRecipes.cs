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

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PlantBasedPizza.Recipes.Serverless
{
    public class ListRecipes
    {
        private readonly IObservabilityService _observability;
        private readonly IRecipeRepository _recipeRepository;

        public ListRecipes()
        {
            Startup.Configure();

            this._observability = Startup.Services.GetRequiredService<IObservabilityService>();
            this._recipeRepository = Startup.Services.GetRequiredService<IRecipeRepository>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            this._observability.AddCorrelationContext(request.Headers);

            this._observability.Info("Retrieved request to list recipes");

            return await this._observability.TraceMethodAsync("List Recipes",
                async () =>
                {
                    var recipes = await this._recipeRepository.List();

                    return new APIGatewayProxyResponse()
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = JsonSerializer.Serialize(recipes),
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
