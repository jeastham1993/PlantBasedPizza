using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.OrderManager.DataTransfer;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public class OrderManagerService : IOrderManagerService
    {
        private readonly OrderManagerDataTransferService _orderRepo;

        public OrderManagerService(OrderManagerDataTransferService orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public async Task<OrderAdapter> GetOrderDetails(string orderIdentifier)
        {
            var order = await this._orderRepo.GetOrderAsync(orderIdentifier).ConfigureAwait(false);

            var orderAdapter = new OrderAdapter();

            foreach (var orderItem in order.OrderItems)
            {
                orderAdapter.Items.Add(new OrderItemAdapter()
                {
                    ItemName = orderItem.ItemName,
                    RecipeIdentifier = orderItem.RecipeIdentifier,
                    Quantity = orderItem.Quantity
                });
            }

            return orderAdapter;
        }
    }
}