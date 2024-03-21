namespace PlantBasedPizza.LoyaltyPoints.IntegrationTest.ViewModels;

public class AddLoyaltyPointsCommand
{
    public string CustomerIdentifier { get; set; }
    
    public string OrderIdentifier { get; set; }
    
    public decimal OrderValue { get; set; }
}