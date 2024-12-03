namespace PlantBasedPizza.OrderManager.Core.OrderReadyForDelivery;

public record OrderReadyForDeliveryCommand
{
    public string OrderIdentifier { get; set; }
}