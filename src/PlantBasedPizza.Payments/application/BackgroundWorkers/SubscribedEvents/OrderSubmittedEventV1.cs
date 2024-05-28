using PlantBasedPizza.Events;

namespace BackgroundWorkers.SubscribedEvents;

public class OrderSubmittedEventV1 : IntegrationEvent
{
    public override string EventName => "order.orderSubmitted";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.plantbasedpizza");
    
    public string CustomerIdentifier { get; init; }
    
    public string OrderIdentifier { get; init; }
    
    public decimal TotalPrice { get; init; }
}