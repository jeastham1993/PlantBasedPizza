using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.Orders.UnitTest;

public class InMemoryOrderRepository : IOrderRepository
{
    private Order order;
    private readonly bool returnInvalidOrder = false;

    public InMemoryOrderRepository(bool returnInvalidOrder = false)
    {
        this.returnInvalidOrder = returnInvalidOrder;
    }
    
    public async Task Add(Order order)
    {
        this.order = order;
    }

    public async Task<Order> Retrieve(string orderIdentifier)
    {
        return returnInvalidOrder ? null : order;
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
        this.order = order;
    }
}