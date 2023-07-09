using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Deliver.Infrastructure;

public class IntegrationEventPublisher : Handles<DriverCollectedOrderEvent>, Handles<OrderDeliveredEvent>
{
    private readonly IEventBus _eventBus;

    public IntegrationEventPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(DriverCollectedOrderEvent evt) => await this._eventBus.Publish(evt);

    public async Task Handle(OrderDeliveredEvent evt) => await this._eventBus.Publish(evt);
}