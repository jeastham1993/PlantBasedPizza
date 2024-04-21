using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;

[AsyncApi]
public class OrderEventPublisher : IOrderEventPublisher
{
    private readonly IEventPublisher _eventPublisher;

    public OrderEventPublisher(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    [Channel("order.orderCompleted.v1")]
    [PublishOperation(typeof(OrderCompletedIntegrationEventV1), Summary = "Published when an order is completed.")]
    public async Task PublishOrderCompletedEventV1(Order order)
    {
        await this._eventPublisher.Publish(new OrderCompletedIntegrationEventV1()
        {
            OrderIdentifier = order.OrderIdentifier,
            CustomerIdentifier = order.CustomerIdentifier,
            OrderValue = order.TotalPrice
        });
    }

    [Channel("order.readyForDelivery.v1")]
    [PublishOperation(typeof(OrderReadyForDeliveryEventV1), Summary = "Published when a delivery order is ready for delivery.")]
    public async Task PublishOrderReadyForDeliveryEventV1(Order order)
    {
        await this._eventPublisher.Publish(new OrderReadyForDeliveryEventV1()
        {
            OrderIdentifier = order.OrderIdentifier,
            DeliveryAddressLine1 = order.DeliveryDetails.AddressLine1,
            DeliveryAddressLine2 = order.DeliveryDetails.AddressLine2,
            DeliveryAddressLine3 = order.DeliveryDetails.AddressLine3,
            DeliveryAddressLine4 = order.DeliveryDetails.AddressLine4,
            DeliveryAddressLine5 = order.DeliveryDetails.AddressLine5,
            Postcode = order.DeliveryDetails.Postcode,
        });
    }

    [Channel("order.orderSubmitted.v1")]
    [PublishOperation(typeof(OrderSubmittedEventV1), Summary = "Published when an order is submitted and paid for.")]
    public async Task PublishOrderSubmittedEventV1(Order order)
    {
        await this._eventPublisher.Publish(new OrderSubmittedEventV1()
        {
            OrderIdentifier = order.OrderIdentifier,
            Items = order.Items.Select(item => new OrderSubmittedEventItem()
            {
                ItemName = item.ItemName,
                RecipeIdentifier = item.RecipeIdentifier
            }).ToList()
        });
    }
}