namespace PlantBasedPizza.Orders.Worker;

public class QueueConfiguration
{
    public string DriverCollectedOrderQueue { get; set; } = "";
    public string OrderQualityCheckedQueue { get; set; } = "";
    public string LoyaltyPointsUpdatedQueue { get; set; } = "";
}