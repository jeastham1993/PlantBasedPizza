using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.Orders.UnitTest;

public class InMemoryOrderRepository : IOrderRepository
{
    private List<Order> _orders = new();
    
    public async Task Add(Order order)
    {
        _orders.Add(order);
    }

    public async Task<Order> Retrieve(string orderIdentifier)
    {
        var order = _orders.Find(o => o.OrderIdentifier == orderIdentifier);

        return order;
    }

    public Task<List<Order>> GetAwaitingCollection()
    {
        throw new NotImplementedException();
    }

    public Task<List<Order>> ForCustomer(string accountId)
    {
        throw new NotImplementedException();
    }

    public async Task Update(Order order)
    {
        _orders = new();
        _orders.Add(order);
    }
}