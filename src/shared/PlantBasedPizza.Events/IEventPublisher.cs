namespace PlantBasedPizza.Events;

public interface IEventPublisher
{
    Task Publish(IntegrationEvent evt);
}