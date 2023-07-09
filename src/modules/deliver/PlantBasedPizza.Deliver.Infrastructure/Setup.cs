using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddDeliveryModuleInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            
            AmazonSQSClient sqsClient = null;
            
            if (chain.TryGetAWSCredentials("dev", out awsCredentials))
            {
                sqsClient = new AmazonSQSClient(awsCredentials, RegionEndpoint.EUWest1);
                services.AddSingleton(new AmazonDynamoDBClient(awsCredentials, RegionEndpoint.EUWest1));   
            }
            else
            {
                sqsClient = new AmazonSQSClient();
                services.AddSingleton(new AmazonDynamoDBClient());
            }

            InfrastructureConstants.OrderReadyForDeliveryQueue = sqsClient.GetQueueUrlAsync(InfrastructureConstants.OrderReadyForDeliveryQueueName).GetAwaiter()
                .GetResult().QueueUrl;

            services.AddSingleton<Handles<OrderDeliveredEvent>, IntegrationEventPublisher>();
            services.AddSingleton<Handles<DriverCollectedOrderEvent>, IntegrationEventPublisher>();
            
            services.AddSingleton<IDeliveryRequestRepository, DeliveryRequestRepository>();
            
            services.AddSingleton<OrderReadyForDeliveryEventHandler>();
            services.AddHostedService<OrderReadyForDeliveryQueueProcessor>();

            return services;
        }
    }
}