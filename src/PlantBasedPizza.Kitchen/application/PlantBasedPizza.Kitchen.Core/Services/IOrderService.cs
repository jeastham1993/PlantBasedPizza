using PlantBasedPizza.Kitchen.Core.Adapters;

namespace PlantBasedPizza.Kitchen.Core.Services;

public interface IOrderService
{
    Task<OrderAdapter> GetOrderDetails(string orderIdentifier);
}