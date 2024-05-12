namespace PlantBasedPizza.Orders.Worker;

public class QueueConfiguration
{
    public string DriverDeliveredOrderQueue { get; set; } = "";
    public string DriverCollectedOrderQueue { get; set; } = "";
    public string OrderPreparingQueue { get; set; } = "";
    public string OrderPrepCompleteQueue { get; set; } = "";
    public string OrderBakedQueue { get; set; } = "";
    public string OrderQualityCheckedQueue { get; set; } = "";
    public string LoyaltyPointsUpdatedQueue { get; set; } = "";
}