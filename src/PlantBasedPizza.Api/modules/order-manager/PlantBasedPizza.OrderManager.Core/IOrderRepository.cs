namespace PlantBasedPizza.OrderManager.Core
{
    public interface IOrderRepository
    {
        Task Add(Order order);

        Task<Order> Retrieve(string orderIdentifier);

        Task<List<Order>> GetAwaitingCollection();
        
        Task Update(Order order);
    }
}