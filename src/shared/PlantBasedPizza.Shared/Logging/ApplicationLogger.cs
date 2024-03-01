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
        private static Logger? _logger;
        
        public static void Init()
        {
            _logger = BuildLoggerConfiguration()
                .CreateLogger();
        }

        public static LoggerConfiguration BuildLoggerConfiguration()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter());
        }
        
        public static void Info(string message)
        {
            if (_logger == null)
            {
                ApplicationLogger.Init();
            }
            using (LogContext.PushProperty("CorrelationId", CorrelationContext.GetCorrelationId()))
                _logger?.Information(message);
        }
        
        public static void Warn(Exception ex, string message)
        {
            if (_logger == null)
            {
                ApplicationLogger.Init();
            }
            
            using (LogContext.PushProperty("CorrelationId", CorrelationContext.GetCorrelationId()))
                _logger?.Warning(ex, message);
        }
        
        public static void Warn(string message)
        {
            if (_logger == null)
            {
                ApplicationLogger.Init();
            }
            
            using (LogContext.PushProperty("CorrelationId", CorrelationContext.GetCorrelationId()))
                _logger?.Warning(message);
        }
        
        public static void Error(Exception ex, string message)
        {
            if (_logger == null)
            {
                ApplicationLogger.Init();
            }
            
            using (LogContext.PushProperty("CorrelationId", CorrelationContext.GetCorrelationId()))
                _logger?.Error(ex, message);
        }
    }
}