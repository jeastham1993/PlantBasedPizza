using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderSubmittedIntegrationEventPublisher : Handles<OrderSubmittedEvent>
{
    private readonly IEventBus _eventBus;

    public OrderSubmittedIntegrationEventPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    
    public async Task Handle(OrderSubmittedEvent evt) => await this._eventBus.Publish(evt);
}