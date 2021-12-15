using System;
using Serilog;
using Serilog.Context;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

namespace PlantBasedPizza.Shared.Logging
{
    public static class ApplicationLogger
    {
        private static ApplicationLogger _applicationLogger;
        
        public static void Init()
        {
            var logger = new LoggerConfiguration()
                .Enrich.WithCorrelationId()
                .WriteTo.Console(new JsonFormatter())
                .WriteTo.File(new JsonFormatter(), "logs/myapp-{Date}.json")
                .CreateLogger();    
        }
        
        public static void Info(string correlationId, string message)
        {
            using (LogContext.PushProperty("CorrelationId", correlationId))
                Log.ForContext(typeof(ApplicationLogger)).Information(message);
        }
        
        public static void Warn(string correlationId, Exception ex, string message)
        {
            using (LogContext.PushProperty("CorrelationId", correlationId))
                Log.ForContext(typeof(ApplicationLogger)).Warning(ex, message);
        }
        
        public static void Error(string correlationId, Exception ex, string message)
        {
            using (LogContext.PushProperty("CorrelationId", correlationId))
                Log.ForContext(typeof(ApplicationLogger)).Error(ex, message);
        }
    }
}