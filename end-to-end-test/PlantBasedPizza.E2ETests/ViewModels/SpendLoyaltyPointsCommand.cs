namespace PlantBasedPizza.E2ETests.ViewModels;

public class SpendLoyaltyPointsCommand
{
    public string CustomerIdentifier { get; set; }
    
    public string OrderIdentifier { get; set; }
    
    public decimal PointsToSpend { get; set; }
}