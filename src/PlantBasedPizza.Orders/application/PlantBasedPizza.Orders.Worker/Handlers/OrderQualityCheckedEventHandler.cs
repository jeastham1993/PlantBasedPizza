using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;
using PlantBasedPizza.Orders.Worker.Notifications;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class OrderQualityCheckedEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserNotificationService _userNotificationService;

        public OrderQualityCheckedEventHandler(IOrderRepository orderRepository, IUserNotificationService userNotificationService)
        {
            _orderRepository = orderRepository;
            _userNotificationService = userNotificationService;
        }
        
        public async Task Handle(OrderQualityCheckedEventV1 evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order quality checked");
            
            await _userNotificationService.NotifyOrderQualityCheckComplete(order.CustomerIdentifier, order.OrderIdentifier);

            if (order.OrderType == OrderType.Delivery)
            {
                order.ReadyForDelivery();
            }
            else
            {
                order.IsAwaitingCollection();
                await _userNotificationService.NotifyReadyForCollection(order.CustomerIdentifier, order.OrderIdentifier);
            }

            await _orderRepository.Update(order);
        }
    }
}