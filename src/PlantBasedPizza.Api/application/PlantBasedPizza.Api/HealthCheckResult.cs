using PlantBasedPizza.OrderManager.Infrastructure;

namespace PlantBasedPizza.Api;

public record HealthCheckResult
{
    public OrderManagerHealthCheckResult OrderManagerHealthCheck { get; set; }
}