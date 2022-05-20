namespace PlantBasedPizza.Kitchen.Api.Services;

public interface IMetrics
{
    Task IncrementMetric(string metricName);
}