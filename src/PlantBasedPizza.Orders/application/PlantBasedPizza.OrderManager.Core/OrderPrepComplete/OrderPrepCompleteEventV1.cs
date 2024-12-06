using PlantBasedPizza.Events;

namespace PlantBasedPizza.OrderManager.Core.OrderPrepComplete;

public class OrderPrepCompleteEventV1 : IntegrationEvent
{
    public override string EventName => "kitchen.orderPrepComplete";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://kitchen.plantbasedpizza");

    public string OrderIdentifier { get; init; } = "";

    public string KitchenIdentifier { get; init; } = "";
}