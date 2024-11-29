using System.Diagnostics;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OutboxItem
{
    public OutboxItem()
    {
        ItemId = Guid.NewGuid().ToString();
        EventTime = DateTime.UtcNow;
        TraceId = Activity.Current?.Id;
    }
    
    public string ItemId { get; set; }
    public string EventType { get; set; } = "";
    
    public string EventData { get; set; } = "";
    
    public DateTime EventTime { get; set; }

    public bool Processed { get; set; } = false;

    public bool Failed { get; set; }
    
    public string? FailureReason { get; set; }
    
    public string? TraceId { get; set; }
}