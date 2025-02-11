using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PlantBasedPizza.Shared;

internal class DisableAllContextPropagator : DistributedContextPropagator
{
    public override IReadOnlyCollection<string> Fields { get; } = new ReadOnlyCollection<string>(new[] { "traceparent" });
    public override IEnumerable<KeyValuePair<string, string?>>? ExtractBaggage(object? carrier, PropagatorGetterCallback? getter)
    {
        return new KeyValuePair<string, string?>[0];
    }
 
    public override void ExtractTraceIdAndState(object? carrier, PropagatorGetterCallback? getter, out string? traceId, out string? traceState)
    {
        traceId = null;
        traceState = null;
    }
 
    public override void Inject(Activity? activity, object? carrier, PropagatorSetterCallback? setter)
    {
        return;
    }
}