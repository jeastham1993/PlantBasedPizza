using System.Text.Json;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Deliver.Core.PublicEvents;

public class DriverCollectedOrderEventV1 : IntegrationEvent
{
    public DriverCollectedOrderEventV1(){}
    public DriverCollectedOrderEventV1(DeliveryRequest request)
    {
        OrderIdentifier = request.OrderIdentifier;
        DriverName = request.Driver;
    }
    
    public override string EventName => "delivery.driverCollectedOrder";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://delivery.plantbasedpizza");
    
    public string DriverName { get; init; }
    
    public string OrderIdentifier { get; init; }

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}