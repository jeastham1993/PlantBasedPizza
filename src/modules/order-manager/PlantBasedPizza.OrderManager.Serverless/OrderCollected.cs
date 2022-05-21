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
using PlantBasedPizza.Recipes.Core.Commands;
using PlantBasedPizza.Recipes.Core.Exceptions;
using PlantBasedPizza.Shared.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PlantBasedPizza.OrderManager.Serverless
{
    public class OrderCollected
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IObservabilityService _observability;

        public OrderCollected()
        {
            Startup.Configure();

            this._observability = Startup.Services.GetRequiredService<IObservabilityService>();
            this._orderRepository = Startup.Services.GetRequiredService<IOrderRepository>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigwRequest, ILambdaContext context)
        {
            this._observability.AddCorrelationContext(apigwRequest.Headers);

            this._observability.Info("Received request to collect an order");

            return await this._observability.TraceMethodAsync("Order Collected",
                async () =>
                {
                    var request = JsonConvert.DeserializeObject<CollectOrderRequest>(apigwRequest.Body);
                    
                    this._observability.Info($"Received {request}");
            
                    var existingOrder = await this._orderRepository.Retrieve(request.OrderIdentifier);

                    if (existingOrder == null)
                    {
                        this._observability.Info($"Existing order ({request.OrderIdentifier}) not found, returning");
                
                        return new APIGatewayProxyResponse()
                        {
                            StatusCode = (int)HttpStatusCode.NotFound,
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
            
                    this._observability.Info($"Order is type {existingOrder.OrderType} and is awaiting collection {existingOrder.AwaitingCollection}");

                    if (existingOrder.OrderType == OrderType.DELIVERY || existingOrder.AwaitingCollection == false)
                    {
                        this._observability.Info("Returning");
                
                        return new APIGatewayProxyResponse()
                        {
                            StatusCode = (int)HttpStatusCode.BadRequest,
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
            
                    this._observability.Info("Order is ready to be completed, marking completed!");

                    existingOrder.AddHistory("Order collected");

                    existingOrder.CompleteOrder();

                    await this._orderRepository.Update(existingOrder).ConfigureAwait(false);

                    this._observability.Info("Updated!");

                    return new APIGatewayProxyResponse()
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = JsonSerializer.Serialize(existingOrder),
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
