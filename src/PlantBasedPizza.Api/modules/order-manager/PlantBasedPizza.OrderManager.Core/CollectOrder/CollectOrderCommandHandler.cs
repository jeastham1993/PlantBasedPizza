using PlantBasedPizza.OrderManager.Core.CompleteOrder;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.CollectOrder;

public class CollectOrderCommandHandler(IOrderRepository orderRepository, CompleteOrderCommandHandler completeOrderCommandHandler)
{
    public async Task<OrderDto?> Handle(CollectOrderRequest command)
    {
        try
        {
            var existingOrder = await orderRepository.Retrieve(command.OrderIdentifier);
            
            if (existingOrder.OrderType == OrderType.Delivery || !existingOrder.AwaitingCollection)
            {
                return new OrderDto(existingOrder);
            }

            await completeOrderCommandHandler.Handle(new CompleteOrderCommand 
            { 
                OrderIdentifier = command.OrderIdentifier,
                CorrelationId = string.Empty
            });

            return new OrderDto(existingOrder);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}