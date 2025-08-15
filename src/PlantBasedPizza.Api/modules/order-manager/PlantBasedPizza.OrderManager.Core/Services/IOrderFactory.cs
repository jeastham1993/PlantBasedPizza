using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.Services;

public interface IOrderFactory
{
    Task<Order> CreateAsync(string orderIdentifier, OrderType type, string customerIdentifier, 
        DeliveryDetails? deliveryDetails = null, string correlationId = "");
}