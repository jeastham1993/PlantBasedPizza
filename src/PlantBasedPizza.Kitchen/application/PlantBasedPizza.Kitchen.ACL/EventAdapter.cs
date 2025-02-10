using Dapr.Client;
using PlantBasedPizza.Kitchen.ACL.Events;
using PlantBasedPizza.Kitchen.Core.OrderConfirmed;

namespace PlantBasedPizza.Kitchen.ACL;

public class EventAdapter(DaprClient client)
{
    private const string KitchenPubSubName = "kitchen";
    
    public async Task Translate(OrderConfirmedEventV1 evt)
    {
        await client.PublishEventAsync(KitchenPubSubName, "order.orderConfirmed.v1", new OrderConfirmed(evt.OrderIdentifier));
    }
}