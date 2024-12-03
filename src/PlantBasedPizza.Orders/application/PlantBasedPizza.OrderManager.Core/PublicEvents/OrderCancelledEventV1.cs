using System.Text.Json;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.OrderManager.Core.PublicEvents;

public class OrderCancelledEventV1 : IntegrationEvent
{
    public override string EventName => "order.orderCancelled";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.plantbasedpizza");
    
    public string OrderIdentifier { get; init; }

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}