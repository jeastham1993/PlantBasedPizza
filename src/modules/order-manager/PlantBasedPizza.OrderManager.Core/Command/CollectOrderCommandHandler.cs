using System.Diagnostics;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.Command;

public class CollectOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public CollectOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    
    public async Task<Order?> Handle(CollectOrderRequest command)
    {
        try
        {
            var existingOrder = await this._orderRepository.Retrieve(command.OrderIdentifier);
            
            if (existingOrder.OrderType == OrderType.DELIVERY || !existingOrder.AwaitingCollection)
            {
                return existingOrder;
            }

            existingOrder.CompleteOrder();

            await this._orderRepository.Update(existingOrder).ConfigureAwait(false);

            return existingOrder;
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}