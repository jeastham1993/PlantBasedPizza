using System.Diagnostics;

namespace PlantBasedPizza.Events;

public class ParseEventResponse<T>
{
    public T? EventData { get; init; }
    
    public string? TraceParent { get; init; }
    
    public ulong TraceId { get; init; }
    
    public ulong SpanId { get; init; }
    
    public int QueueTime { get; init; }
    
    public DateTime EventPublishDate { get; init; }
    
    public string? EventId { get; init; }
    
    public string MessageId { get; init; }
    
    public string ReceiptHandle { get; init; }
}