namespace PlantBasedPizza.OrderManager.Core.ExternalEvents
{
    public record OrderDeliveredEvent
    {
        public string OrderIdentifier { get; init; } = "";
    }
}