using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace PlantBasedPizza.Events;

public abstract class IntegrationEvent
{
    [System.Text.Json.Serialization.JsonIgnore]
    [JsonIgnore]
    public abstract string EventName { get; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    [JsonIgnore]
    public abstract string EventVersion { get; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    [JsonIgnore]
    public abstract Uri Source { get; }
}