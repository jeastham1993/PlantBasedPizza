using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.Deliver.Infrastructure.IntegrationEvents;

[AsyncApi]
public class DeliveryEventPublisher : IDeliveryEventPublisher
{
    private readonly IEventPublisher _eventPublisher;

    public DeliveryEventPublisher(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    [Channel("delivery.driverCollectedOrder.v1")]
    [PublishOperation(typeof(DriverCollectedOrderEventV1), Summary = "Published when a driver collects an order.")]
    public async Task PublishDriverOrderCollectedEventV1(DeliveryRequest deliveryRequest)
    {
        await this._eventPublisher.Publish(new DriverCollectedOrderEventV1()
        {
            DriverName = deliveryRequest.Driver,
            OrderIdentifier = deliveryRequest.OrderIdentifier
        });
    }

    [Channel("delivery.driverDeliveredOrder.v1")]
    [PublishOperation(typeof(DriverDeliveredOrderEventV1), Summary = "Published when a driver delivers an order.")]
    public async Task PublishDriverDeliveredOrderEventV1(DeliveryRequest deliveryRequest)
    {
        await this._eventPublisher.Publish(new DriverDeliveredOrderEventV1()
        {
            OrderIdentifier = deliveryRequest.OrderIdentifier
        });
    }
}