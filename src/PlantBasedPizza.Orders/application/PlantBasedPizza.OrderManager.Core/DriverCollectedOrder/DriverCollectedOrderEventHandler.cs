using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.DriverCollectedOrder
{
    public class DriverCollectedOrderEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserNotificationService _userNotificationService;

        public DriverCollectedOrderEventHandler(IOrderRepository orderRepository, IUserNotificationService userNotificationService)
        {
            _orderRepository = orderRepository;
            _userNotificationService = userNotificationService;
        }
        
        [Channel("delivery.driverCollectedOrder.v1")]
        [PublishOperation(typeof(DriverCollectedOrderEventV1), OperationId = nameof(DriverCollectedOrderEventV1))]
        public async Task Handle(DriverCollectedOrderEventV1 evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory($"Order collected by driver {evt.DriverName}");
            
            await _orderRepository.Update(order).ConfigureAwait(false);
            await _userNotificationService.NotifyOrderDriverAssigned(order.CustomerIdentifier, order.OrderIdentifier);
        }
    }
}