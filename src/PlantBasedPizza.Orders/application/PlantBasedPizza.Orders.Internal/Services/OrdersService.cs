using System.Diagnostics;
using System.Text.Json;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.Orders.Internal.Services;

public class OrdersService : Orders.OrdersBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDistributedCache _cache;

    public OrdersService(IOrderRepository orderRepository, IDistributedCache cache)
    {
        _orderRepository = orderRepository;
        _cache = cache;
    }

    public override async Task<GetOrderDetailsReply> GetOrderDetails(GetOrderDetailsRequest request, ServerCallContext context)
    {
        var cachedOrderData = await _cache.GetAsync(request.OrderIdentifier);

        if (cachedOrderData != null)
        {
            Activity.Current?.AddTag("cached", "true");
            var cachedOrder = JsonSerializer.Deserialize<OrderDto>(cachedOrderData);
            
            var reply = new GetOrderDetailsReply()
            {
                OrderIdentifier = cachedOrder.OrderIdentifier,
                OrderValue = Convert.ToDouble(cachedOrder.TotalPrice)
            };

            foreach (var item in cachedOrder.Items)
            {
                reply.Items.Add(new OrderDetailItem()
                {
                    ItemName = item.ItemName,
                    RecipeIdentifier = item.RecipeIdentifier
                });
            }

            return reply;
        }
        
        Activity.Current?.AddTag("cached", "false");
        
        var order = await _orderRepository.Retrieve(request.OrderIdentifier).ConfigureAwait(false);
            
        var orderReply = new GetOrderDetailsReply()
        {
            OrderIdentifier = order.OrderIdentifier,
            OrderValue = Convert.ToDouble(order.TotalPrice)
        };

        foreach (var item in order.Items)
        {
            orderReply.Items.Add(new OrderDetailItem()
            {
                ItemName = item.ItemName,
                RecipeIdentifier = item.RecipeIdentifier
            });
        }

        return orderReply;
    }
}