namespace PlantBasedPizza.Shared.Logging
{
    public class ObservabiityService : IObservabilityService
    {
        public void Info(string message)
        {
            ApplicationLogger.Info(message);
        }
        
        public void Warn(Exception ex, string message)
        {
            ApplicationLogger.Warn(ex, message);
        }
        
        public void Error(Exception ex, string message)
        {
            ApplicationLogger.Error(ex, message);
        }
    }
}