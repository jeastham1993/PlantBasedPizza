using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.IntegrationEvents;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.CollectOrder;

public class CollectOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoyaltyPointService _loyaltyPointService;
    private readonly IEventPublisher _eventPublisher;

    public CollectOrderCommandHandler(IOrderRepository orderRepository, ILoyaltyPointService loyaltyPointService, IEventPublisher eventPublisher)
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
            
            // await this._loyaltyPointService.AddLoyaltyPoints(
            //     existingOrder.CustomerIdentifier,
            //     existingOrder.OrderIdentifier,
            //     existingOrder.TotalPrice);

            await this._eventPublisher.Publish(new OrderCompletedIntegrationEventV1()
            {
                OrderIdentifier = existingOrder.OrderIdentifier,
                CustomerIdentifier = existingOrder.CustomerIdentifier,
                OrderValue = existingOrder.TotalPrice
            });

            await this._orderRepository.Update(existingOrder).ConfigureAwait(false);

            return new OrderDto(existingOrder);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}