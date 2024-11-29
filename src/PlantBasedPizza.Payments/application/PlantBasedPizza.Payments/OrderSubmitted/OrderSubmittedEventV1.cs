using System.Text.Json;
using System.Text.Json.Serialization;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Payments.ExternalEvents;

public class OrderSubmittedEventV1 : IntegrationEvent
{
    public override string EventName => "order.orderSubmitted";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.plantbasedpizza");
    
    [JsonPropertyName("OrderIdentifier")]
    public string OrderIdentifier { get; set; }

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}