using PlantBasedPizza.Events;

namespace PlantBasedPizza.Delivery.IntegrationTests.ViewModels;

public class OrderReadyForDeliveryEventV1 : IntegrationEvent
{
    public string OrderIdentifier { get; init; } = "";
    public string DeliveryAddressLine1 { get; init; } = "";
        
    public string DeliveryAddressLine2 { get; init; } = "";
        
    public string DeliveryAddressLine3 { get; init; } = "";
        
    public string DeliveryAddressLine4 { get; init; } = "";
        
    public string DeliveryAddressLine5 { get; init; } = "";
        
    public string Postcode { get; init; } = "";
    public override string EventName => "order.readyForDelivery";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://delivery.plantbasedpizza");
}