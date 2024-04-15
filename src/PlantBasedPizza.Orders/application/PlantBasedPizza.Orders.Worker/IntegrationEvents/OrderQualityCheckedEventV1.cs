using PlantBasedPizza.Events;

namespace PlantBasedPizza.Orders.Worker.IntegrationEvents;

public class OrderQualityCheckedEventV1 : IntegrationEvent
{
    public override string EventName => "kitchen.orderCreated";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://kitchen.plantbasedpizza");

    public string OrderIdentifier { get; init; } = "";

    public string KitchenIdentifier { get; init; } = "";
}