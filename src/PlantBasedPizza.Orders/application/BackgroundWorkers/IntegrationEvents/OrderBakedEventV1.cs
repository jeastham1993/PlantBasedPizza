using PlantBasedPizza.Events;

namespace BackgroundWorkers.IntegrationEvents;

public class OrderBakedEventV1 : IntegrationEvent
{
    public override string EventName => "kitchen.orderBaked";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.test.plantbasedpizza");

    public string OrderIdentifier { get; init; } = "";

    public string KitchenIdentifier { get; init; } = "";
}