using PlantBasedPizza.Events;

namespace PlantBasedPizza.Kitchen.Infrastructure.IntegrationEvents;

public class KitchenConfirmedOrderEventV1 : IntegrationEvent
{
    public override string EventName => "kitchen.orderConfirmed";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://kitchen.plantbasedpizza");

    public string OrderIdentifier { get; init; } = "";

    public string KitchenIdentifier { get; init; } = "";
}