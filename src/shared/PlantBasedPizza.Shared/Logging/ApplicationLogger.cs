using System;
using Serilog;
using Serilog.Context;
using Serilog.Core;
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
                .WriteTo.Console(new JsonFormatter())
                .WriteTo.File(new JsonFormatter(), "logs/myapp-{Date}.json")
                .CreateLogger();
        }
        
        public static void Info(string correlationId, string message)
        {
            using (LogContext.PushProperty("CorrelationId", correlationId))
                _logger.Information(message);
        }
        
        public static void Warn(string correlationId, Exception ex, string message)
        {
            using (LogContext.PushProperty("CorrelationId", correlationId))
                _logger.Warning(ex, message);
        }
        
        public static void Error(string correlationId, Exception ex, string message)
        {
            using (LogContext.PushProperty("CorrelationId", correlationId))
                _logger.Error(ex, message);
        }
    }
}