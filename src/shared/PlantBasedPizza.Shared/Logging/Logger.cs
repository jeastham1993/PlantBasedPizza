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
    }
}