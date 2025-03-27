using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace PlantBasedPizza.Recipes.Functions;

public class BackgroundWorkers()
{
    [Function("HandleNewRecipe")]
    public async Task HandleNewRecipe(
        [ServiceBusTrigger("recipes.recipeCreated.v1", Connection = "AZURE_SERVICE_BUS_CONNECTION_STRING")]
        ServiceBusReceivedMessage message
    )
    {
        Console.WriteLine(message.MessageId);
    }
}