using PlantBasedPizza.Events;

namespace PlantBasedPizza.Kitchen.Infrastructure.IntegrationEvents;

public class OrderPreparingEventV1 : IntegrationEvent
{
    public override string EventName => "kitchen.orderPreparing";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://kitchen.plantbasedpizza");

    public string OrderIdentifier { get; init; } = "";

    public string KitchenIdentifier { get; init; } = "";
}