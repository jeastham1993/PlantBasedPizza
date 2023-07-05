namespace PlantBasedPizza.Shared.Events;

public interface IEventBus
{
    public Task Publish(IntegrationEvent evt);
}