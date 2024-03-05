using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.GetDelivery;

public record AddressDTO
{
    public AddressDTO(Address address)
    {
        this.AddressLine1 = address.AddressLine1;
        this.AddressLine2 = address.AddressLine2;
        this.AddressLine3 = address.AddressLine3;
        this.AddressLine4 = address.AddressLine4;
        this.AddressLine5 = address.AddressLine5;
        this.Postcode = address.Postcode;
    }
    public string AddressLine1 { get; set; }
    
    public string AddressLine2 { get; set; }
    
    public string AddressLine3 { get; set; }
    
    public string AddressLine4 { get; set; }
    
    public string AddressLine5 { get; set; }
    
    public string Postcode { get; set; }
}