using PlantBasedPizza.Events;

namespace PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;

public class OrderSubmittedEventV1 : IntegrationEvent
{
    public override string EventName => "order.orderSubmitted";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.plantbasedpizza");
    
    public string OrderIdentifier { get; init; }
    public string CustomerIdentifier { get; init; }
    public decimal TotalPrice {get;init;}
    
    public List<OrderSubmittedEventItem> Items { get; init; }
}

public record OrderSubmittedEventItem
{
    public string ItemName { get; init; } = "";
    public string RecipeIdentifier { get; init; } = "";
    public decimal UnitPrice {get; init;}
    public decimal Quantity {get; init;}
}