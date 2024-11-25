namespace PlantBasedPizza.Deliver.Core.OrderReadyForDelivery;

public class OrderReadyForDeliveryEventV1
{
    public string OrderIdentifier { get; init; } = "";
    
    public string DeliveryAddressLine1 { get; init; } = "";
        
    public string DeliveryAddressLine2 { get; init; } = "";
        
    public string DeliveryAddressLine3 { get; init; } = "";
        
    public string DeliveryAddressLine4 { get; init; } = "";
        
    public string DeliveryAddressLine5 { get; init; } = "";
        
    public string Postcode { get; init; } = "";
}