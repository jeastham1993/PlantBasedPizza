using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class IntegrationEventPublisher : Handles<OrderSubmittedEvent>, Handles<OrderReadyForDeliveryEvent>
{
    private readonly IEventBus _eventBus;

    public IntegrationEventPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    
    public async Task Handle(OrderSubmittedEvent evt) => await this._eventBus.Publish(evt);
    public async Task Handle(OrderReadyForDeliveryEvent evt) => await this._eventBus.Publish(evt);
}