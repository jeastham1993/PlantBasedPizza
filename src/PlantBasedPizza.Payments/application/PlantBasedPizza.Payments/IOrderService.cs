namespace PlantBasedPizza.Payments;

public interface IOrderService
{
    Task<Order> GetOrderDetails(string orderIdentifier);
}