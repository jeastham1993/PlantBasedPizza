using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.DataTransfer;

public class OrderManagerDataTransferService(IOrderRepository orderRepository)
{
    public async Task<OrderDto> GetOrderAsync(string orderId)
    {
        var order = await orderRepository.Retrieve(orderId);

        return new OrderDto(order.OrderIdentifier, order.Items.Select(item => new OrderItemDto(item.RecipeIdentifier, item.ItemName, item.Quantity)).ToList());
    }
}