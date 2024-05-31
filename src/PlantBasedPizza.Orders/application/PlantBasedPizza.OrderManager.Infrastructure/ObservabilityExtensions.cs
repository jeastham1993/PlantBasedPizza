using System.Diagnostics;
using Datadog.Trace;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public static class ObservabilityExtensions
{
    public static void AddToTelemetry(this AddItemToOrderCommand command)
    {
        if (Activity.Current is null)
        {
            return;
        }

        Tracer.Instance.ActiveScope?.Span.SetTag("orderIdentifier", command.OrderIdentifier);
        Tracer.Instance.ActiveScope?.Span.SetTag("recipeIdentifier", command.RecipeIdentifier);
        Tracer.Instance.ActiveScope?.Span.SetTag("quantity", command.Quantity);
    }
}