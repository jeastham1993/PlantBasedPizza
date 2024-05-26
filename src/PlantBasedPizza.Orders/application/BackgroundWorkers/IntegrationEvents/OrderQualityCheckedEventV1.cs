using PlantBasedPizza.Events;

namespace BackgroundWorkers.IntegrationEvents;

public class OrderQualityCheckedEventV1 : IntegrationEvent
{
    public override string EventName => "kitchen.orderQualityChecked";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.test.plantbasedpizza");

    public string OrderIdentifier { get; init; } = "";

    public string KitchenIdentifier { get; init; } = "";
}