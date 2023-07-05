namespace PlantBasedPizza.Shared.Events;

public abstract class IntegrationEvent
{
    public virtual string EventName { get; }
}