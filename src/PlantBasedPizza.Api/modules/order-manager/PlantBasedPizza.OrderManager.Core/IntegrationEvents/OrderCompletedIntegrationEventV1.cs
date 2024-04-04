using PlantBasedPizza.Events;

namespace PlantBasedPizza.OrderManager.Core.IntegrationEvents;

public class OrderCompletedIntegrationEvent : IntegrationEvent
{
    public override string EventName { get; }
    public override string EventVersion { get; }
}