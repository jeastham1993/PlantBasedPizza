using System.Text.Json;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Deliver.Core.PublicEvents;

public class DriverDeliveredOrderEventV1 : IntegrationEvent
{
    public DriverDeliveredOrderEventV1(){}
    
    public DriverDeliveredOrderEventV1(DeliveryRequest request)
    {
        OrderIdentifier = request.OrderIdentifier;
    }
    
    public override string EventName => "delivery.driverDeliveredOrder";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://delivery.plantbasedpizza");
    
    public string OrderIdentifier { get; init; }

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}