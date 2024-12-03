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

    public void AddToTelemetry(string eventId)
    {
        var eventString = AsString();
        
        var eventBytes = Encoding.UTF8.GetBytes(eventString);
        
        Activity.Current?.AddTag("messaging.message.id", eventId);
        Activity.Current?.AddTag("messaging.message.body.size", eventBytes.Length);

        try
        {
            var jsonSchema = JsonSchema.FromSampleJson(eventString);
            Activity.Current?.AddTag("messaging.message.schema", jsonSchema.ToJson());
        }
        catch
        {
            // Don't want a schema parsing failure to cause the service to fail
        }
        
        Activity.Current?.AddTag("messaging.operation.name", "send");
        Activity.Current?.AddTag("messaging.operation.type", "send");
        Activity.Current?.AddTag("messaging.system", "dapr.pubsub");
        Activity.Current?.AddTag("messaging.batch.message_count", 1);
        Activity.Current?.AddTag("messaging.destination.name", $"{EventName}.{EventVersion}");
    }
}