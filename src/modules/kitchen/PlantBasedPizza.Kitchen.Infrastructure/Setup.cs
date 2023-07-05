using System.Collections.Immutable;
using Amazon;
using Amazon.CloudWatch;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Handlers;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddKitchenInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IRecipeService, RecipeService>();
            services.AddSingleton<IOrderManagerService, OrderManagerService>();
            services.AddSingleton<IKitchenRequestRepository, KitchenRequestRepository>();
            services.AddSingleton<OrderSubmittedEventHandler>();
            
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;

            AmazonSQSClient sqsClient = null;
            
            if (chain.TryGetAWSCredentials("dev", out awsCredentials))
            {
                services.AddSingleton(new AmazonCloudWatchClient(awsCredentials, RegionEndpoint.EUWest1));

                sqsClient = new AmazonSQSClient(awsCredentials, RegionEndpoint.EUWest1);
                
                services.AddSingleton(sqsClient);
            }
            else
            {
                services.AddSingleton(new AmazonCloudWatchClient());

                sqsClient = new AmazonSQSClient();
                
                services.AddSingleton(sqsClient);
                services.AddSingleton(new AmazonSQSClient());
            }

            InfrastructureConstants.OrderSubmittedQueueUrl = sqsClient.GetQueueUrlAsync(InfrastructureConstants.OrderSubmittedQueueName).GetAwaiter()
                .GetResult().QueueUrl;

            services.AddHostedService<OrderSubmittedQueueProcessor>();

            return services;
        }
    }
}