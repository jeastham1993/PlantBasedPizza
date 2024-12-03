using System.Text.Json;
using System.Text.Json.Serialization;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.OrderManager.Core.PublicEvents;

public class OrderReadyForDeliveryEventV1 : IntegrationEvent
{
    [JsonIgnore]
    public override string EventName => "order.readyForDelivery";
    
    [JsonIgnore]
    public override string EventVersion => "v1";
    
    [JsonIgnore]
    public override Uri Source => new Uri("https://orders.plantbasedpizza");

    public string OrderIdentifier { get; init; } = "";
    public string DeliveryAddressLine1 { get; init; } = "";
        
    public string DeliveryAddressLine2 { get; init; } = "";
        
    public string DeliveryAddressLine3 { get; init; } = "";
        
    public string DeliveryAddressLine4 { get; init; } = "";
        
    public string DeliveryAddressLine5 { get; init; } = "";
        
    public string Postcode { get; init; } = "";

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}