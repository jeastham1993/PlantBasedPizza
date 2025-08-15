namespace PlantBasedPizza.Shared.Events;

public interface IDomainEventDispatcher
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent;
}