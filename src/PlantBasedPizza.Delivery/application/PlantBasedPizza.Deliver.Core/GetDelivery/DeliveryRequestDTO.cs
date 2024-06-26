
using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.GetDelivery;

public class DeliveryRequestDto
{
    public DeliveryRequestDto(DeliveryRequest request)
    {
        this.OrderIdentifier = request.OrderIdentifier;
        this.Driver = request.Driver;
        this.AwaitingCollection = request.AwaitingCollection;
        this.DeliveryAddress = new AddressDto(request.DeliveryAddress);
        this.DriverCollectedOn = request.DriverCollectedOn;
        this.DeliveredOn = request.DeliveredOn;
    }
    
    public string OrderIdentifier { get; set; }
    
    public string Driver { get; set; }
        
    public bool AwaitingCollection { get; set; }
    
    public AddressDto? DeliveryAddress { get; set; }
    
    public DateTime? DriverCollectedOn { get; set; }
    
    public DateTime? DeliveredOn { get; set; }
}