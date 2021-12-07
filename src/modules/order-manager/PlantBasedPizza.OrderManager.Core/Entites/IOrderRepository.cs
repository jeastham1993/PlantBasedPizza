using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantBasedPizza.OrderManager.Core.Entites
{
    public interface IOrderRepository
    {
        Task Add(Order order);

        Task<Order> Retrieve(string orderIdentifier);

        Task<List<Order>> GetAwaitingCollection();
        
        Task Update(Order order);
    }
}