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
                "dd_env",
                Environment.GetEnvironmentVariable("ENV"));
        var serviceProperty = propertyFactory
            .CreateProperty(
                "dd_service",
                Environment.GetEnvironmentVariable("SERVICE_NAME"));
        var traceProperty = propertyFactory
            .CreateProperty(
                "dd_traceId",
                Activity.Current?.TraceId.ToString());
        var spanProperty = propertyFactory
            .CreateProperty(
                "dd_spanId",
                Activity.Current?.SpanId.ToString());
        
        logEvent.AddOrUpdateProperty(envProperty);
        logEvent.AddOrUpdateProperty(serviceProperty);
        logEvent.AddOrUpdateProperty(traceProperty);
        logEvent.AddOrUpdateProperty(spanProperty);
    }
}