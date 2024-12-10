using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using NJsonSchema;

namespace PlantBasedPizza.Events;

public abstract class IntegrationEvent
{
    [JsonIgnore]
    public abstract string EventName { get; }
    
    [JsonIgnore]
    public abstract string EventVersion { get; }
    
    [JsonIgnore]
    public abstract Uri Source { get; }

    public virtual string AsString() => "";
}