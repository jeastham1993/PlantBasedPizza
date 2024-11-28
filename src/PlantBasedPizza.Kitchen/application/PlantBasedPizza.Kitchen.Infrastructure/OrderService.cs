using Grpc.Core;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Orders.Internal;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class OrderService : IOrderService
{
    private readonly Metadata _metadata;
    private readonly Orders.Internal.Orders.OrdersClient _orders;

    public OrderService(Orders.Internal.Orders.OrdersClient orders)
    {
        _orders = orders;
        _metadata = new Metadata()
        {
            { "dapr-app-id", "orders-internal" }
        };
    }

    public async Task<OrderAdapter> GetOrderDetails(string orderIdentifier)
    {
        var result =
            await _orders.GetOrderDetailsAsync(new GetOrderDetailsRequest()
            {
                OrderIdentifier = orderIdentifier
            }, _metadata);

        var adapter = new OrderAdapter();
        foreach (var item in result.Items)
        {
            adapter.Items.Add(new OrderItemAdapter()
            {
                ItemName = item.ItemName,
                RecipeIdentifier = item.RecipeIdentifier
            });
        }

        return adapter;
    }
}