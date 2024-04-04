namespace PlantBasedPizza.Events;

public interface IEventSubscriber<T> where T : IEventHandler
{
    Task RetrieveEvents(string queueName, string eventName, T handler);
}