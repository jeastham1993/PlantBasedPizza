namespace PlantBasedPizza.OrderManager.Core;

public interface IOrderFactory
{
    Task<Order> CreateAsync(OrderType type, string customerIdentifier, 
        DeliveryDetails? deliveryDetails = null, string correlationId = "");
}