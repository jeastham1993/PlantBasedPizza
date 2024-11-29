using Dapr.Client;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.PublicEvents;
using Saunter.Attributes;

namespace PlantBasedPizza.Deliver.Infrastructure;

[AsyncApi]
public class DeliveryEventPublisher(DaprClient daprClient) : IDeliveryEventPublisher
{
    private const string SOURCE = "delivery";
    
    [Channel("delivery.driverCollectedOrder.v1")]
    [PublishOperation(typeof(DriverCollectedOrderEventV1), Summary = "Published when a driver collects an order.")]
    public async Task PublishDriverOrderCollectedEventV1(DriverCollectedOrderEventV1 evt)
    {
        var eventMetadata = new Dictionary<string, string>(2)
        {
            { "cloudevent.source", SOURCE },
            { "cloudevent.type", "kitchen.driverCollectedOrder.v1" },
            { "cloudevent.id", Guid.NewGuid().ToString() }
        };
        
        await daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt, eventMetadata);
    }

    [Channel("delivery.driverDeliveredOrder.v1")]
    [PublishOperation(typeof(DriverDeliveredOrderEventV1), Summary = "Published when a driver delivers an order.")]
    public async Task PublishDriverDeliveredOrderEventV1(DriverDeliveredOrderEventV1 evt)
    {
        var eventMetadata = new Dictionary<string, string>(2)
        {
            { "cloudevent.source", SOURCE },
            { "cloudevent.type", "kitchen.driverDeliveredOrder.v1" },
            { "cloudevent.id", Guid.NewGuid().ToString() }
        };
        
        await daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt, eventMetadata);
    }
}