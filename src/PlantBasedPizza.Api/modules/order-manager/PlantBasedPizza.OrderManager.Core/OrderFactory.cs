using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.OrderManager.Core;

public class OrderFactory(IDomainEventDispatcher eventDispatcher, ILogger<OrderFactory> logger)
    : IOrderFactory
{
    private readonly IDomainEventDispatcher _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));

    public async Task<Order> CreateAsync(OrderType type, string customerIdentifier,
        DeliveryDetails? deliveryDetails = null, string correlationId = "")
    {
        Guard.AgainstNullOrEmpty(customerIdentifier, nameof(customerIdentifier));

        if (type == OrderType.Delivery && deliveryDetails == null)
            throw new ArgumentException("If order type is delivery a delivery address must be specified",
                nameof(deliveryDetails));

        logger.LogInformation($"Creating a new order with type {type}");
        Activity.Current?.AddTag("order.type", type.ToString());
        
        var orderIdentifier = Guid.NewGuid().ToString();

        var order = new Order(orderIdentifier, type, customerIdentifier, deliveryDetails);
        
        Activity.Current?.AddTag("order.id", orderIdentifier);
        Activity.Current?.AddTag("customer.id", customerIdentifier);

        await _eventDispatcher.PublishAsync(new OrderCreatedEvent(orderIdentifier)
        {
            CorrelationId = correlationId
        });

        return order;
    }
}