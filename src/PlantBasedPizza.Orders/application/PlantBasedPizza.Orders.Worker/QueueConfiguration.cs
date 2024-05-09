namespace PlantBasedPizza.Orders.Worker;

public class QueueConfiguration
{
    public string DriverCollectedOrderQueueUrl { get; set; } = "";
    public string OrderQualityCheckedQueueUrl { get; set; } = "";
    public string LoyaltyPointsUpdatedQueueUrl { get; set; } = "";
}