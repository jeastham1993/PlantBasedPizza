using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Recipes.Core.Commands;
using PlantBasedPizza.Recipes.Core.Exceptions;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Serverless
{
    public class GetOrdersAwaitingCollection
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IObservabilityService _logger;

        public GetOrdersAwaitingCollection()
        {
            Startup.Configure();

            this._logger = Startup.Services.GetRequiredService<IObservabilityService>();
            this._orderRepository = Startup.Services.GetRequiredService<IOrderRepository>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apiRequest, ILambdaContext context)
        {
            this._logger.AddCorrelationContext(apiRequest.Headers);

            this._logger.Info("Received request to get an order");

            return await this._logger.TraceMethodAsync("Get Order",
                async () =>
                {
                    var awaitingCollection = await this._orderRepository.GetAwaitingCollection();

                    return new APIGatewayProxyResponse()
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = JsonConvert.SerializeObject(awaitingCollection),
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
