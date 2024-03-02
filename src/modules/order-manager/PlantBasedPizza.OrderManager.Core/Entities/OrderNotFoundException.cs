namespace PlantBasedPizza.OrderManager.Core.Entities;

public class OrderNotFoundException(string orderNumber) : Exception($"Order with number {orderNumber} not found.")
{
    public string OrderNumber { get; init; } = orderNumber;
}