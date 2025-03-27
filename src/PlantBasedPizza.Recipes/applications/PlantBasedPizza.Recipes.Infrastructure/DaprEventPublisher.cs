using Dapr.Client;
using Microsoft.Extensions.Configuration;
using PlantBasedPizza.Recipes.Core.IntegrationEvents;

namespace PlantBasedPizza.Recipes.Infrastructure;

public class DaprEventPublisher(IConfiguration configuration, DaprClient daprClient) : IEventPublisher
{
    public async Task Publish(RecipeCreatedEventV1 evt)
    {
        await daprClient.PublishEventAsync(configuration["DAPR_PUB_SUB_NAME"], $"{evt.EventName}.{evt.EventVersion}", evt);
    }
}