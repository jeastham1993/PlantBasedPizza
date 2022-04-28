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
    public class SubmitOrder
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IObservabilityService _observability;

        public SubmitOrder()
        {
            Startup.Configure();

            this._observability = Startup.Services.GetRequiredService<IObservabilityService>();
            this._orderRepository = Startup.Services.GetRequiredService<IOrderRepository>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigwRequest, ILambdaContext context)
        {
            this._observability.AddCorrelationContext(apigwRequest.Headers);

            this._observability.Info("Received request to submit an order");

            return await this._observability.TraceMethodAsync("Order Collected",
                async () =>
                {
                    var order = await this._orderRepository.Retrieve(apigwRequest.PathParameters["orderIdentifier"]);

                    order.SubmitOrder();

                    await this._orderRepository.Update(order);

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
