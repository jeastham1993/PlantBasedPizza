using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using PlantBasedPizza.Kitchen.Api.Services;

namespace PlantBasedPizza.Kitchen.Api;

public class CloudWatchMetrics : IMetrics
{
    private readonly AmazonCloudWatchClient _cloudWatchClient;

    public CloudWatchMetrics(AmazonCloudWatchClient cloudWatchClient)
    {
        _cloudWatchClient = cloudWatchClient;
    }

    public async Task IncrementMetric(string metricName)
    {
        await this._cloudWatchClient.PutMetricDataAsync(new PutMetricDataRequest()
        {
            Namespace = "KitchenService",
            MetricData = new List<MetricDatum>()
            {
                new MetricDatum()
                {
                    Unit = StandardUnit.Count,
                    Value = 1,
                    MetricName = metricName,
                }
            }
        });
    }
}