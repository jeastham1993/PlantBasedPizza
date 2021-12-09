using System;
using Serilog;

namespace PlantBasedPizza.Shared.Logging
{
    public static class Logger
    {
        public static void Init()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();    
        }
        
        public static void Info(string message)
        {
            Log.Information(message);
        }
        
        public static void Warn(Exception ex, string message)
        {
            Log.Warning(ex, message);
        }
        
        public static void Error(Exception ex, string message)
        {
            Log.Error(ex, message);
        }
    }
}