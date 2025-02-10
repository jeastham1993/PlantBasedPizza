namespace PlantBasedPizza.OrderManager.Core.OrderDelivered
{
    public record OrderDeliveredEvent
    {
        public string OrderIdentifier { get; init; } = "";
    }
}