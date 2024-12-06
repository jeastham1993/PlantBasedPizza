using System.Text.Json.Serialization;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Kitchen.Core.OrderConfirmed;

public class OrderConfirmedEventV1 : IntegrationEvent
{
    public override string EventName => "order.orderConfirmed";
    public override string EventVersion => "v1";
    public override Uri Source => new("https://orders.plantbasedpizza");

    [JsonPropertyName("OrderIdentifier")]
    public string OrderIdentifier { get; init; }
}