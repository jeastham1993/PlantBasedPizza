using System.Text.Json;
using System.Text.Json.Serialization;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.OrderManager.Core.PublicEvents;

public class OrderCompletedIntegrationEventV1 : IntegrationEvent
{
    [JsonIgnore]
    public override string EventName => "order.orderCompleted";
    
    [JsonIgnore]
    public override string EventVersion => "v1";
    
    [JsonIgnore]
    public override Uri Source => new Uri("https://orders.plantbasedpizza");

    public string OrderIdentifier { get; set; } = "";
    
    public string CustomerIdentifier { get; set; } = "";
    
    public decimal OrderValue { get; set; }

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}