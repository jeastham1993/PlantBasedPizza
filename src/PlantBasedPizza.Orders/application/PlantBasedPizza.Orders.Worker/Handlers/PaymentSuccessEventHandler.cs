using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;
using PlantBasedPizza.Orders.Worker.Notifications;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class PaymentSuccessEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserNotificationService _notificationService;

        public PaymentSuccessEventHandler(IOrderRepository orderRepository, IUserNotificationService notificationService)
        {
            _orderRepository = orderRepository;
            _notificationService = notificationService;
        }
        
        public async Task Handle(PaymentSuccessfulEventV1 evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            order.Confirm(evt.Amount);

            await _orderRepository.Update(order);
            
            await _notificationService.NotifyPaymentSuccess(order.CustomerIdentifier, evt.OrderIdentifier);
        }
    }
}