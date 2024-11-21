using Dapr.Client;
using PlantBasedPizza.Deliver.Core.Entities;
using Saunter.Attributes;

namespace PlantBasedPizza.Deliver.Infrastructure.IntegrationEvents;

[AsyncApi]
public class DeliveryEventPublisher : IDeliveryEventPublisher
{
    private readonly DaprClient _daprClient;

    public DeliveryEventPublisher(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    [Channel("delivery.driverCollectedOrder.v1")]
    [PublishOperation(typeof(DriverCollectedOrderEventV1), Summary = "Published when a driver collects an order.")]
    public async Task PublishDriverOrderCollectedEventV1(DeliveryRequest deliveryRequest)
    {
        var evt = new DriverCollectedOrderEventV1()
        {
            DriverName = deliveryRequest.Driver,
            OrderIdentifier = deliveryRequest.OrderIdentifier
        };
        
        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("delivery.driverDeliveredOrder.v1")]
    [PublishOperation(typeof(DriverDeliveredOrderEventV1), Summary = "Published when a driver delivers an order.")]
    public async Task PublishDriverDeliveredOrderEventV1(DeliveryRequest deliveryRequest)
    {
        var evt = new DriverDeliveredOrderEventV1()
        {
            OrderIdentifier = deliveryRequest.OrderIdentifier
        };
        
        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
    }
}