using PlantBasedPizza.OrderManager.Core.OrderSubmitted;
using PlantBasedPizza.OrderManager.Core.PublicEvents;

namespace PlantBasedPizza.OrderManager.Core.Entities;

public interface IOrderEventPublisher
{
    Task PublishOrderSubmittedEventV1(OrderSubmittedEventV1 evt);
    
    Task PublishOrderCompletedEventV1(OrderCompletedIntegrationEventV1 evt);
    
    Task PublishOrderReadyForDeliveryEventV1(OrderReadyForDeliveryEventV1 evt);
    
    Task PublishOrderConfirmedEventV1(OrderConfirmedEventV1 evt);

    Task PublishOrderCreatedEventV1(OrderCreatedEventV1 evt);
}