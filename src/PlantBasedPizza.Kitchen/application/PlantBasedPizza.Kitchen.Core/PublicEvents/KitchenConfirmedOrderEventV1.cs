using System.Text.Json;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Kitchen.Core.PublicEvents;

public class KitchenConfirmedOrderEventV1 : IntegrationEvent
{
    public override string EventName => "kitchen.orderConfirmed";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://kitchen.plantbasedpizza");

    public string OrderIdentifier { get; set; } = "";

    public string KitchenIdentifier { get; set; } = "";

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}