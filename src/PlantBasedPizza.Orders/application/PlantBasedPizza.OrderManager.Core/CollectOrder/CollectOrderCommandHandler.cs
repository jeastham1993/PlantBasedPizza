using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;

namespace PlantBasedPizza.OrderManager.Core.CollectOrder;

public class CollectOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderEventPublisher _eventPublisher;

    public CollectOrderCommandHandler(IOrderRepository orderRepository, ILoyaltyPointService loyaltyPointService, IOrderEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
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

            await this._eventPublisher.PublishOrderCompletedEventV1(existingOrder);

            await this._orderRepository.Update(existingOrder).ConfigureAwait(false);

            return new OrderDto(existingOrder);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}