using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PlantBasedPizza.OrderManager.Core.Command;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Shared.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PlantBasedPizza.OrderManager.Serverless
{
    public class AddItemToOrder
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRecipeService _recipeService;
        private readonly IObservabilityService _logger;

        public AddItemToOrder()
        {
            Startup.Configure();

            this._logger = Startup.Services.GetRequiredService<IObservabilityService>();
            this._recipeService = Startup.Services.GetRequiredService<IRecipeService>();
            this._orderRepository = Startup.Services.GetRequiredService<IOrderRepository>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apiRequest, ILambdaContext context)
        {
            this._logger.AddCorrelationContext(apiRequest.Headers);

            this._logger.Info("Received request to get an order");

            return await this._logger.TraceMethodAsync("Get Order",
                async () =>
                {
                    var request = JsonConvert.DeserializeObject<AddItemToOrderCommand>(apiRequest.Body);
                    
                    var recipe = await this._recipeService.GetRecipe(request.RecipeIdentifier);
            
                    var order = await this._orderRepository.Retrieve(request.OrderIdentifier);

                    order.AddOrderItem(request.RecipeIdentifier, recipe.ItemName, request.Quantity, recipe.Price);

                    await this._orderRepository.Update(order);

                    return new APIGatewayProxyResponse()
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = JsonConvert.SerializeObject(order),
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
