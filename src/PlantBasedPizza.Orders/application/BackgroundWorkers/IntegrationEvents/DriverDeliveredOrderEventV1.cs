using PlantBasedPizza.Events;

namespace BackgroundWorkers.IntegrationEvents;

public class DriverDeliveredOrderEventV1 : IntegrationEvent
{
    public override string EventName => "delivery.driverDeliveredOrder";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.test.plantbasedpizza");
    
    public string OrderIdentifier { get; init; }
}