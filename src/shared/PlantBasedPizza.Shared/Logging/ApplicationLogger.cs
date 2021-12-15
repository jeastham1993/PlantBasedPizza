using System;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

namespace PlantBasedPizza.Shared.Logging
{
    public static class ApplicationLogger
    {
        private static Logger _logger;
        
        public static void Init()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(), "logs/myapp-{Date}.json")
                .CreateLogger();
        }
        
        public static void Info(string message)
        {
            using (LogContext.PushProperty("CorrelationId", CorrelationContext.GetCorrelationId()))
                _logger.Information(message);
        }
        
        public static void Warn(Exception ex, string message)
        {
            using (LogContext.PushProperty("CorrelationId", CorrelationContext.GetCorrelationId()))
                _logger.Warning(ex, message);
        }
        
        public static void Warn(string message)
        {
            using (LogContext.PushProperty("CorrelationId", CorrelationContext.GetCorrelationId()))
                _logger.Warning(message);
        }
        
        public static void Error(Exception ex, string message)
        {
            using (LogContext.PushProperty("CorrelationId", CorrelationContext.GetCorrelationId()))
                _logger.Error(ex, message);
        }
    }
}