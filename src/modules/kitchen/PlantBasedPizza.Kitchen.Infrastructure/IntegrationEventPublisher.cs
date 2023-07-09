using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class IntegrationEventPublisher: Handles<OrderBakedEvent>, Handles<OrderPreparingEvent>, Handles<OrderPrepCompleteEvent>, Handles<OrderQualityCheckedEvent>
{
    private readonly IEventBus _eventBus;

    public IntegrationEventPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(OrderBakedEvent evt) => await this._eventBus.Publish(evt);

    public async Task Handle(OrderPreparingEvent evt) => await this._eventBus.Publish(evt);

    public async Task Handle(OrderPrepCompleteEvent evt)=> await this._eventBus.Publish(evt);

    public async Task Handle(OrderQualityCheckedEvent evt)=> await this._eventBus.Publish(evt);
}