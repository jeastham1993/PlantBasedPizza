using PlantBasedPizza.OrderManager.Infrastructure;
using Temporalio.Client;
using Temporalio.Extensions.Hosting;
using Temporalio.Extensions.OpenTelemetry;

namespace PlantBasedPizza.Orders.Worker;

public static class TemporalConfiguration
{
    public static IServiceCollection AddTemporalWorkflows(this IServiceCollection services,
        IConfiguration configuration)
    {
        var temporalEndpoint = configuration["TEMPORAL_ENDPOINT"];

        if (string.IsNullOrEmpty(temporalEndpoint)) return services;

        services.AddTemporalClient(options =>
        {
            options.TargetHost = temporalEndpoint!;
            options.Tls = (configuration["TEMPORAL_TLS"] ?? "") == "true" ? new TlsOptions() : null;
            options.Namespace = "default";
            options.Interceptors = new[] { new TracingInterceptor() };
        });
        services.AddHostedTemporalWorker("orders-queue")
            .AddSingletonActivities<OrderActivities>()
            .AddWorkflow<OrderProcessingWorkflow>();

        return services;
    }
}