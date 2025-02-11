using Dapr.Client;
using PlantBasedPizza.Kitchen.ACL.Events;
using PlantBasedPizza.Kitchen.Core.OrderConfirmed;

namespace PlantBasedPizza.Kitchen.ACL;

public class EventAdapter(DaprClient client, ILogger<EventAdapter> logger)
{
    private const string KitchenPubSubName = "kitchen";
    
    public async Task Translate(OrderConfirmedEventV1 evt)
    {
        logger.LogInformation("Processing {evtType}", nameof(OrderConfirmedEventV1));
        
        await client.PublishEventAsync(KitchenPubSubName, "internal.kitchen.orderConfirmed.v1", new OrderConfirmed(evt.OrderIdentifier));
    }
}