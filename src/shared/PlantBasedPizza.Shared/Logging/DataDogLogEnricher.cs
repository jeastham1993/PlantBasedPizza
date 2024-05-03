using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace PlantBasedPizza.Shared.Logging;

public class DataDogLogEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var envProperty = propertyFactory
            .CreateProperty(
                "dd.env",
                Environment.GetEnvironmentVariable("ENV"));
        var serviceProperty = propertyFactory
            .CreateProperty(
                "dd.service",
                Environment.GetEnvironmentVariable("SERVICE_NAME"));
        var traceProperty = propertyFactory
            .CreateProperty(
                "dd.trace_id",
                Activity.Current?.TraceId.ToString());
        var spanProperty = propertyFactory
            .CreateProperty(
                "dd.span_id",
                Activity.Current?.SpanId.ToString());
        
        logEvent.AddOrUpdateProperty(envProperty);
        logEvent.AddOrUpdateProperty(serviceProperty);
        logEvent.AddOrUpdateProperty(traceProperty);
        logEvent.AddOrUpdateProperty(spanProperty);
    }
}