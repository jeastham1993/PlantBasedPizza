namespace PlantBasedPizza.OrderManager.Core.Entities
{
    public interface IOrderRepository
    {
        Task Add(Order order);

        Task<Order> Retrieve(string orderIdentifier);

        Task<List<Order>> GetAwaitingCollection();
        
        Task<List<Order>> ForCustomer(string accountId);
        
        Task Update(Order order);
    }
}