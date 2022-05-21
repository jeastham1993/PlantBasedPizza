using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PlantBasedPizza.OrderManager.Core.Command;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Serverless
{
    public class CreatePickupOrder
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IObservabilityService _observability;

        public CreatePickupOrder()
        {
            Startup.Configure();

            this._observability = Startup.Services.GetRequiredService<IObservabilityService>();
            this._orderRepository = Startup.Services.GetRequiredService<IOrderRepository>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigwRequest, ILambdaContext context)
        {
            this._observability.AddCorrelationContext(apigwRequest.Headers);

            this._observability.Info("Received request to get a recipe");

            return await this._observability.TraceMethodAsync("Get Recipes",
                async () =>
                {
                    var request = JsonConvert.DeserializeObject<CreatePickupOrderCommand>(apigwRequest.Body);
                        
                    var existingOrder = await this._orderRepository.Retrieve(request.OrderIdentifier);

                    if (existingOrder != null)
                    {
                        return new APIGatewayProxyResponse()
                        {
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Body = "Order exists",
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

                    var order = Order.Create(request.OrderIdentifier, request.OrderType, request.CustomerIdentifier,
                        null,
                        CorrelationContext.GetCorrelationId());

                    await this._orderRepository.Add(order);

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
