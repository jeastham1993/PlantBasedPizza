using System.Diagnostics;
using Dapr.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PlantBasedPizza.OrderManager.Infrastructure.HealthChecks;

public class RecipeServiceHealthCheck(
    IOptions<ServiceEndpoints> serviceEndpoints,
    ILogger<RecipeServiceHealthCheck> logger) : IHealthCheck
{
    private readonly HttpClient _httpClient = DaprClient.CreateInvokeHttpClient();

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        var isHealthy = true;
        var healthCheckResult = new Dictionary<string, object>();

        try
        {
            var res = await _httpClient.GetAsync($"http://{serviceEndpoints.Value.Recipes}/recipes/health",
                cancellationToken);

            healthCheckResult.Add("recipe.api", res.IsSuccessStatusCode ? "healthy" : "unhealthy");

            Activity.Current?.AddTag("recipe.api.status", res.StatusCode);
        }
        catch (Exception ex)
        {
            isHealthy = false;
            logger.LogWarning(ex, "Healthcheck failure for recipe service");

            Activity.Current?.AddTag("recipe.health", false);
            healthCheckResult.Add("recipe.api", "unhealthy");
        }

        if (isHealthy) return HealthCheckResult.Healthy("Healthy", healthCheckResult);

        return new HealthCheckResult(
            context.Registration.FailureStatus, "Service unhealthy.", null, healthCheckResult);
    }
}