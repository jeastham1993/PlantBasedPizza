
namespace PlantBasedPizza.Deliver.Core.Entities;

public class DeliveryRequestDto
{
    public DeliveryRequestDto(DeliveryRequest request)
    {
        OrderIdentifier = request.OrderIdentifier;
        Driver = request.Driver;
        AwaitingCollection = request.AwaitingCollection;
        DeliveryAddress = new AddressDto(request.DeliveryAddress);
        DriverCollectedOn = request.DriverCollectedOn;
        DeliveredOn = request.DeliveredOn;
    }
    
    public string OrderIdentifier { get; set; }
    
    public string Driver { get; set; }
        
    public bool AwaitingCollection { get; set; }
    
    public AddressDto? DeliveryAddress { get; set; }
    
    public DateTime? DriverCollectedOn { get; set; }
    
    public DateTime? DeliveredOn { get; set; }
}