namespace PlantBasedPizza.OrderManager.Core.Entities;

public interface IOrderEventPublisher
{
    Task PublishOrderCompletedEventV1(Order order);
    
    Task PublishOrderReadyForDeliveryEventV1(Order order);
    
    Task PublishOrderSubmittedEventV1(Order order);
    
    Task PublishOrderConfirmedEventV1(Order order);
}