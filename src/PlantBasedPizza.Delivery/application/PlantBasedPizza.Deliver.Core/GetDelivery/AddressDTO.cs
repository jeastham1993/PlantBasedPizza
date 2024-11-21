using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.GetDelivery;

public record AddressDto
{
    public AddressDto(Address address)
    {
        AddressLine1 = address.AddressLine1;
        AddressLine2 = address.AddressLine2;
        AddressLine3 = address.AddressLine3;
        AddressLine4 = address.AddressLine4;
        AddressLine5 = address.AddressLine5;
        Postcode = address.Postcode;
    }
    public string AddressLine1 { get; set; }
    
    public string AddressLine2 { get; set; }
    
    public string AddressLine3 { get; set; }
    
    public string AddressLine4 { get; set; }
    
    public string AddressLine5 { get; set; }
    
    public string Postcode { get; set; }
}