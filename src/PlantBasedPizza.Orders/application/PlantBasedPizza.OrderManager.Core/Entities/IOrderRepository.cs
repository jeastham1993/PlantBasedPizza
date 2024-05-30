namespace PlantBasedPizza.OrderManager.Core.Entities
{
    public interface IOrderRepository
    {
        Task Add(Order order);

        Task<Order> Retrieve(string customerIdentifier, string orderIdentifier);
        
        Task<Order> RetrieveByOrderId(string orderIdentifier);
        
        Task<bool> Exists(string customerIdentifier, string orderIdentifier);

        Task<List<Order>> GetAwaitingCollection();
        
        Task Update(Order order);
    }
}