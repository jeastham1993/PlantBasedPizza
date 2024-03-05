using System.Threading.Tasks;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public class OrderManagerService : IOrderManagerService
    {
        private readonly IOrderRepository _orderRepo;

        public OrderManagerService(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public async Task<OrderAdapter> GetOrderDetails(string orderIdentifier)
        {
            var order = await this._orderRepo.Retrieve(orderIdentifier).ConfigureAwait(false);

            var orderAdapter = new OrderAdapter();

            foreach (var orderItem in order.Items)
            {
                orderAdapter.Items.Add(new OrderItemAdapter()
                {
                    ItemName = orderItem.ItemName,
                    RecipeIdentifier = orderItem.RecipeIdentifier
                });
            }

            return orderAdapter;
        }
    }
}