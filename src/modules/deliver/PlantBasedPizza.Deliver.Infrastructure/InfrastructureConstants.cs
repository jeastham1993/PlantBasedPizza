namespace PlantBasedPizza.Deliver.Infrastructure;

public static class InfrastructureConstants
{
    public static string TableName => Environment.GetEnvironmentVariable("TABLE_NAME") ?? "plant-based-pizza";
    
    public static string OrderReadyForDeliveryQueueName => Environment.GetEnvironmentVariable("ORDER_SUBMITTED_QUEUE") ?? "delivery-order-ready-for-delivery";
    
    public static string OrderReadyForDeliveryQueue { get; set; }
}