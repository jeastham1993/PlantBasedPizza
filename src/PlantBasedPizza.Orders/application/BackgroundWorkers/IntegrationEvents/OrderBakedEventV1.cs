using System.Text.Json.Serialization;
using PlantBasedPizza.Events;

namespace BackgroundWorkers.IntegrationEvents;

public class OrderBakedEventV1 : IntegrationEvent
{
    public override string EventName => "kitchen.orderBaked";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.test.plantbasedpizza");

    [JsonPropertyName("orderIdentifier")]
    public string OrderIdentifier { get; init; } = "";

    [JsonPropertyName("kitchenIdentifier")]
    public string KitchenIdentifier { get; init; } = "";
}