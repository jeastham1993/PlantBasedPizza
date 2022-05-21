using Amazon.DynamoDBv2;
using Amazon.StepFunctions;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Kitchen.Infrastructure;

namespace PlantBasedPizza.Kitchen.Orchestrator;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();

        services.AddSingleton<AmazonStepFunctionsClient>();
        services.AddSingleton<AmazonDynamoDBClient>(new AmazonDynamoDBClient());
        services.AddTransient<IKitchenRequestRepository, KitchenRequestRepository>();
        services.AddTransient<IWorkflowManager, StepFunctionsWorkflowManager>();
    }
}