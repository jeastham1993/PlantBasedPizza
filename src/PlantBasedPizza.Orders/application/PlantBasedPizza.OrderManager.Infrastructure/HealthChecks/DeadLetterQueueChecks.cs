using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace PlantBasedPizza.OrderManager.Infrastructure.HealthChecks;

public class DeadLetterQueueChecks(
    ILogger<DeadLetterQueueChecks> logger,
    IDeadLetterRepository deadLetterRepository) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        var isHealthy = true;
        var healthCheckResult = new Dictionary<string, object>();
        try
        {
            var deadLetterItems = await deadLetterRepository.GetDeadLetterItems();

            healthCheckResult.Add("deadletters", deadLetterItems.Count());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Healthcheck failure for dead letter queue");

            Activity.Current?.AddTag("orders.deadLetter.healthy", false);
            healthCheckResult.Add("deadletters", -1);
        }

        if (isHealthy) return HealthCheckResult.Healthy("Healthy", healthCheckResult);

        return new HealthCheckResult(
            context.Registration.FailureStatus, "Service unhealthy.", null, healthCheckResult);
    }
}