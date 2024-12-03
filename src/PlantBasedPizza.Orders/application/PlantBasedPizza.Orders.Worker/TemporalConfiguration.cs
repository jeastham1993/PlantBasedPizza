using PlantBasedPizza.OrderManager.Infrastructure;
using Temporalio.Extensions.Hosting;

namespace PlantBasedPizza.Orders.Worker;

public static class TemporalConfiguration
{
    public static IServiceCollection AddTemporalWorkflows(this IServiceCollection services,
        IConfiguration configuration)
    {
        var temporalEndpoint = configuration["TEMPORAL_ENDPOINT"];

        if (string.IsNullOrEmpty(temporalEndpoint)) return services;

        services.AddHostedTemporalWorker("orders-queue")
            .AddSingletonActivities<OrderActivities>()
            .AddWorkflow<OrderProcessingWorkflow>();

        return services;
    }
}