using PlantBasedPizza.Events;

namespace PlantBasedPizza.Kitchen.Infrastructure.IntegrationEvents;

public class OrderQualityCheckedEventV1 : IntegrationEvent
{
    public override string EventName => "kitchen.qualityChecked";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://kitchen.plantbasedpizza");

    public string OrderIdentifier { get; init; } = "";

    public string KitchenIdentifier { get; init; } = "";
}