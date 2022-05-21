using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Orchestrator;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        
        services.AddSingleton<AmazonDynamoDBClient>(new AmazonDynamoDBClient());
        services.AddTransient<IKitchenRequestRepository, KitchenRequestRepository>();
    }
}