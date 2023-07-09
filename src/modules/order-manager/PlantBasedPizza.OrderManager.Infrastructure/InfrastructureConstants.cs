namespace PlantBasedPizza.OrderManager.Infrastructure;

public static class InfrastructureConstants
{
    public static string TableName => Environment.GetEnvironmentVariable("TABLE_NAME") ?? "plant-based-pizza";
    
    public static string OrderPreparingQueueName => Environment.GetEnvironmentVariable("ORDER_SUBMITTED_QUEUE") ?? "order-manager-order-preparing";
    
    public static string OrderPreparingQueueUrl { get; set; }
    
    public static string OrderPrepCompleteQueueName => Environment.GetEnvironmentVariable("ORDER_SUBMITTED_QUEUE") ?? "order-manager-prep-complete";
    
    public static string OrderPrepCompleteQueueUrl { get; set; }
    
    public static string OrderBakedQueueName => Environment.GetEnvironmentVariable("ORDER_SUBMITTED_QUEUE") ?? "order-manager-order-baked";
    
    public static string OrderBakedQueueUrl { get; set; }
    
    public static string OrderQualityCheckedQueueName => Environment.GetEnvironmentVariable("ORDER_SUBMITTED_QUEUE") ?? "order-manager-quality-checked";
    
    public static string OrderQualityCheckedQueueUrl { get; set; }
    
    public static string OrderDeliveredQueueName => Environment.GetEnvironmentVariable("ORDER_SUBMITTED_QUEUE") ?? "order-manager-order-delivered";
    
    public static string OrderDeliveredQueueUrl { get; set; }
    
    public static string DriverCollectedQueueName => Environment.GetEnvironmentVariable("ORDER_SUBMITTED_QUEUE") ?? "order-manager-driver-collected";
    
    public static string DriverCollectedQueueUrl { get; set; }
}