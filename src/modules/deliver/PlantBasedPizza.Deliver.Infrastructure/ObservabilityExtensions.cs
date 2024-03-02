using System.Diagnostics;
using PlantBasedPizza.Deliver.Core.Commands;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Deliver.Infrastructure;

public static class ObservabilityExtensions
{
    public static void AddToTelemetry(this AssignDriverRequest request)
    {
        if (Activity.Current is null)
        {
            return;
        }

        Activity.Current.SetTag("orderIdentifier", request.OrderIdentifier);
        Activity.Current.SetTag("driverName", request.DriverName);
    }
    
    public static void AddToTelemetry(this MarkOrderDeliveredRequest request)
    {
        if (Activity.Current is null)
            return;

        Activity.Current.SetTag("orderIdentifier", request.OrderIdentifier);
    }
}