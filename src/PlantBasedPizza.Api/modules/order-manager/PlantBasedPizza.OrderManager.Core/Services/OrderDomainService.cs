using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Core.Services;

public class OrderDomainService : IOrderDomainService
{
    private readonly IDomainEventDispatcher _eventDispatcher;

    public OrderDomainService(IDomainEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    public async Task SubmitOrderAsync(Order order, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(order);

        if (!order.Items.Any()) 
            throw new ArgumentException("Cannot submit an order with no items");

        order.MarkAsSubmitted();
        order.AddHistory("Submitted order.");

        await _eventDispatcher.PublishAsync(new OrderSubmittedEvent(order.OrderIdentifier)
        {
            CorrelationId = correlationId
        });
    }

    public async Task CompleteOrderAsync(Order order, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(order);

        order.MarkAsCompleted();
        order.AddHistory("Order completed.");

        var evt = new OrderCompletedEvent(order.CustomerIdentifier, order.OrderIdentifier, order.TotalPrice)
        {
            CorrelationId = correlationId
        };

        await _eventDispatcher.PublishAsync(evt);
        order.AddIntegrationEvent(evt);
    }

    public async Task MarkOrderAwaitingCollectionAsync(Order order, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(order);

        order.MarkAsAwaitingCollection();
        order.AddHistory("Order awaiting collection");

        // Note: No event needed for this operation based on current domain logic
    }
}