using PlantBasedPizza.Events;

namespace BackgroundWorkers.IntegrationEvents;

public class DriverCollectedOrderEventV1 : IntegrationEvent
{
    public override string EventName => "delivery.driverCollectedOrder";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.test.plantbasedpizza");
    
    public string DriverName { get; init; }
    
    public string OrderIdentifier { get; init; }
}