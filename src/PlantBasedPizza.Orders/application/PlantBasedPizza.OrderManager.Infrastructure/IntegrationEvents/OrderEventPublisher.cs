using Dapr.Client;
using PlantBasedPizza.OrderManager.Core.Entities;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;

[AsyncApi]
public class OrderEventPublisher : IOrderEventPublisher
{
    private readonly DaprClient _daprClient;

    public OrderEventPublisher(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    [Channel("order.orderCompleted.v1")]
    [PublishOperation(typeof(OrderCompletedIntegrationEventV1), Summary = "Published when an order is completed.")]
    public async Task PublishOrderCompletedEventV1(Order order)
    {
        var evt = new OrderCompletedIntegrationEventV1()
        {
            OrderIdentifier = order.OrderIdentifier,
            CustomerIdentifier = order.CustomerIdentifier,
            OrderValue = order.TotalPrice
        };

        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("order.readyForDelivery.v1")]
    [PublishOperation(typeof(OrderReadyForDeliveryEventV1), Summary = "Published when a delivery order is ready for delivery.")]
    public async Task PublishOrderReadyForDeliveryEventV1(Order order)
    {
        var evt = new OrderReadyForDeliveryEventV1()
        {
            OrderIdentifier = order.OrderIdentifier,
            DeliveryAddressLine1 = order.DeliveryDetails.AddressLine1,
            DeliveryAddressLine2 = order.DeliveryDetails.AddressLine2,
            DeliveryAddressLine3 = order.DeliveryDetails.AddressLine3,
            DeliveryAddressLine4 = order.DeliveryDetails.AddressLine4,
            DeliveryAddressLine5 = order.DeliveryDetails.AddressLine5,
            Postcode = order.DeliveryDetails.Postcode,
        };

        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("order.orderSubmitted.v1")]
    [PublishOperation(typeof(OrderSubmittedEventV1), Summary = "Published when an order is submitted and paid for.")]
    public async Task PublishOrderSubmittedEventV1(Order order)
    {
        var evt = new OrderSubmittedEventV1()
        {
            OrderIdentifier = order.OrderIdentifier,
            Items = order.Items.Select(item => new OrderSubmittedEventItem()
            {
                ItemName = item.ItemName,
                RecipeIdentifier = item.RecipeIdentifier
            }).ToList()
        };

        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
    }
}