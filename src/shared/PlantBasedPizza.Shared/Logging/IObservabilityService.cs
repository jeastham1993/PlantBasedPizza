using System;
using System.Threading.Tasks;

namespace PlantBasedPizza.Shared.Logging
{
    public interface IObservabilityService
    {
        void Info(string message);

        void Warn(Exception ex, string message);

        void Error(Exception ex, string message);
    }
}