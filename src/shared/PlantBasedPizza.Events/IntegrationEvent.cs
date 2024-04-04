namespace PlantBasedPizza.Events;

public abstract class IntegrationEvent
{
    public abstract string EventName { get; }
    
    public abstract string EventVersion { get; }
    
    public abstract string Source { get; }
}