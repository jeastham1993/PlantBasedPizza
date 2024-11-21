namespace PlantBasedPizza.LoyaltyPoints.Shared.Core;

public class AddLoyaltyPointsCommand
{
    public string CustomerIdentifier { get; set; }
    
    public string OrderIdentifier { get; set; }
    
    public decimal OrderValue { get; set; }

    public bool Validate()
    {
        return !string.IsNullOrEmpty(OrderIdentifier) && !string.IsNullOrEmpty(CustomerIdentifier);
    }
}