using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using PlantBasedPizza.Recipes.Core.OrderCompletedHandler;

namespace PlantBasedPizza.Recipes.Functions;

public class BackgroundWorkers(OrderCompletedHandler orderCompletedHandler)
{
    [Function("HandleOrderCompleted")]
    public async Task HandleNewRecipe(
        [ServiceBusTrigger("order.orderCompleted.v2",
            "recipe-service",
            Connection = "AZURE_SERVICE_BUS_CONNECTION_STRING",
            AutoCompleteMessages = true)]
        ServiceBusReceivedMessage message)
    {
        var messageBody = message.Body.ToString();
        var evt = JsonSerializer.Deserialize<OrderCompletedEventV2>(messageBody);

        await orderCompletedHandler.Handle(evt);
    }
}