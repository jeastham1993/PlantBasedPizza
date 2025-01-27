using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.CollectOrder;

public class CollectOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoyaltyPointService _loyaltyPointService;
    private readonly OrderEventPublisher _eventPublisher;

    public CollectOrderCommandHandler(IOrderRepository orderRepository, ILoyaltyPointService loyaltyPointService, OrderEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _loyaltyPointService = loyaltyPointService;
        _eventPublisher = eventPublisher;
    }
    
    public async Task<OrderDto?> Handle(CollectOrderRequest command)
    {
        try
        {
            var existingOrder = await this._orderRepository.Retrieve(command.OrderIdentifier);
            
            if (existingOrder.OrderType == OrderType.Delivery || !existingOrder.AwaitingCollection)
            {
                return new OrderDto(existingOrder);
            }

            existingOrder.CompleteOrder();
            
            await this._loyaltyPointService.AddLoyaltyPoints(
                existingOrder.CustomerIdentifier,
                existingOrder.OrderIdentifier,
                existingOrder.TotalPrice);

            await this._orderRepository.Update(existingOrder).ConfigureAwait(false);
            await _eventPublisher.Publish(new OrderCompletedEvent(existingOrder.CustomerIdentifier, existingOrder.OrderIdentifier, existingOrder.TotalPrice));

            return new OrderDto(existingOrder);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}