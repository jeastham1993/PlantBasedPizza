using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;

public interface IOrderEventPublisher
{
    Task PublishOrderCompletedEventV1(Order order);
    
    Task PublishOrderReadyForDeliveryEventV1(Order order);
    
    Task PublishOrderSubmittedEventV1(Order order);
}