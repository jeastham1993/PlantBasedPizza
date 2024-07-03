using System.Text.Json.Serialization;
using PlantBasedPizza.Events;

namespace BackgroundWorkers.IntegrationEvents;

public class OrderPreparingEventV1 : IntegrationEvent
{
    public override string EventName => "kitchen.orderPreparing";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.test.plantbasedpizza");

    [JsonPropertyName("orderIdentifier")]
    public string OrderIdentifier { get; init; } = "";

    [JsonPropertyName("kitchenIdentifier")]
    public string KitchenIdentifier { get; init; } = "";
}