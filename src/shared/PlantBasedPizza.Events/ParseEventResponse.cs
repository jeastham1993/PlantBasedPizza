namespace PlantBasedPizza.Events;

public class ParseEventResponse<T>
{
    public T? EventData { get; init; }
    
    public string? TraceParent { get; init; }
    
    public int QueueTime { get; init; }
    
    public string? EventId { get; init; }
}