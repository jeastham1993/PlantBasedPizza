using System.Text.Json;
using Azure.Messaging.ServiceBus;
using PlantBasedPizza.Recipes.Core.IntegrationEvents;

namespace PlantBasedPizza.Recipes.Infrastructure;

public class ServiceBusEventPublisher(ServiceBusClient serviceBusClient) : IEventPublisher
{
    public async Task Publish(RecipeCreatedEventV1 evt)
    {
        var sender = serviceBusClient.CreateSender("recipes.recipeCreated.v1");

        using var messageBatch = await sender.CreateMessageBatchAsync();

        messageBatch.TryAddMessage(new ServiceBusMessage(JsonSerializer.Serialize(evt)));

        await sender.SendMessagesAsync(messageBatch);

        await sender.DisposeAsync();
    }
}