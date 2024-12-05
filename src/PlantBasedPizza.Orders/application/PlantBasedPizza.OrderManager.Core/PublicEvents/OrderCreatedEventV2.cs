using System.Text.Json;
using System.Text.Json.Serialization;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.OrderManager.Core.PublicEvents;

public class OrderCreatedEventV2 : IntegrationEvent
{
    [JsonIgnore]
    public override string EventName => "order.orderCreated";
    
    [JsonIgnore]
    public override string EventVersion => "v2";
    
    [JsonIgnore]
    public override Uri Source => new Uri("https://orders.plantbasedpizza");

    public string OrderId { get; set; } = "";

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}