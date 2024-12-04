using System.Diagnostics;
using Grpc.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace PlantBasedPizza.OrderManager.Infrastructure.HealthChecks;

public class LoyaltyServiceHealthChecks(
    ILogger<LoyaltyServiceHealthChecks> logger,
    Loyalty.LoyaltyClient loyaltyClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        var isHealthy = true;
        var healthCheckResult = new Dictionary<string, object>();
        
        try
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));
            var metadata = new Metadata
            {
                { "dapr-app-id", "loyaltyinternal" }
            };
            await loyaltyClient.GetCustomerLoyaltyPointsAsync(new GetCustomerLoyaltyPointsRequest
                { CustomerIdentifier = "james" }, metadata);

            healthCheckResult.Add("loyalty.api", "healthy");
        }
        catch (Exception ex)
        {
            isHealthy = false;
            logger.LogWarning(ex, "Healthcheck failure for internal loyalty service");

            Activity.Current?.AddTag("loyalty.gRPC.healthy", false);
            healthCheckResult.Add("loyalty.api", "unhealthy");
        }

        if (isHealthy) return HealthCheckResult.Healthy("Healthy", healthCheckResult);

        return new HealthCheckResult(
            context.Registration.FailureStatus, "Service unhealthy.", null, healthCheckResult);
    }
}