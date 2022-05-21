using System.Collections.Generic;
using System.Net;
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
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PlantBasedPizza.OrderManager.Serverless
{
    public class CreateDeliveryOrder
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IObservabilityService _observability;

        public CreateDeliveryOrder()
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
                    var request = JsonConvert.DeserializeObject<Core.Command.CreateDeliveryOrder>(apigwRequest.Body);
                        
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
                        new DeliveryDetails()
                        {
                            AddressLine1 = request.AddressLine1,
                            AddressLine2 = request.AddressLine2,
                            AddressLine3 = request.AddressLine3,
                            AddressLine4 = request.AddressLine4,
                            AddressLine5 = request.AddressLine5,
                            Postcode = request.Postcode,
                        }, CorrelationContext.GetCorrelationId());

                    await this._orderRepository.Add(order);

                    return new APIGatewayProxyResponse()
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = JsonSerializer.Serialize(order),
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
