namespace PlantBasedPizza.Kitchen.Infrastructure;

public static class InfrastructureConstants
{
    public static string TableName => Environment.GetEnvironmentVariable("TABLE_NAME") ?? "plant-based-pizza";
    
    public static string OrderSubmittedQueueName => Environment.GetEnvironmentVariable("ORDER_SUBMITTED_QUEUE") ?? "kitchen-order-submitted";
    
    public static string OrderSubmittedQueueUrl { get; set; }
}