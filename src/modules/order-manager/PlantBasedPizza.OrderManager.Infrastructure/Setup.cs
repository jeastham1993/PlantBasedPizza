using Amazon;
using Amazon.CloudWatch;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Core.Handlers;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddOrderManagerInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddSingleton<IRecipeService, RecipeService>();

            services.AddSingleton<OrderPrepCompleteEventHandler>();
            services.AddSingleton<OrderPreparingEventHandler>();
            services.AddSingleton<OrderBakedEventHandler>();
            services.AddSingleton<OrderQualityCheckedEventHandler>();
            services.AddSingleton<DriverDeliveredOrderEventHandler>();
            services.AddSingleton<DriverCollectedOrderEventHandler>();
            
            services.AddSingleton<InboundEventQueueProcessor<OrderPreparingEvent, OrderPreparingEventHandler>>();
            services.AddSingleton<InboundEventQueueProcessor<OrderPrepCompleteEvent, OrderPrepCompleteEventHandler>>();
            services.AddSingleton<InboundEventQueueProcessor<OrderBakedEvent, OrderBakedEventHandler>>();
            services.AddSingleton<InboundEventQueueProcessor<OrderQualityCheckedEvent, OrderQualityCheckedEventHandler>>();
            services.AddSingleton<InboundEventQueueProcessor<DriverCollectedOrderEvent, DriverCollectedOrderEventHandler>>();
            services.AddSingleton<InboundEventQueueProcessor<OrderDeliveredEvent, DriverDeliveredOrderEventHandler>>();

            services.AddSingleton<Handles<OrderSubmittedEvent>, IntegrationEventPublisher>();
            services.AddSingleton<Handles<OrderReadyForDeliveryEvent>, IntegrationEventPublisher>();

            LoadQueueUrls();

            services.AddHostedService<OrderPreparingQueueProcessor>();
            services.AddHostedService<OrderPrepCompleteQueueProcessor>();
            services.AddHostedService<OrderBakedQueueProcessor>();
            services.AddHostedService<OrderQualityCheckedQueueProcessor>();
            services.AddHostedService<OrderDeliveredQueueProcessor>();
            services.AddHostedService<DriverCollectedOrderQueueProcessor>();

            return services;
        }
    
        private static void LoadQueueUrls()
        {
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
        
            AmazonSQSClient sqsClient = null;
            
            if (chain.TryGetAWSCredentials("dev", out awsCredentials))
            {
                sqsClient = new AmazonSQSClient(awsCredentials, RegionEndpoint.EUWest1);
            }
            else
            {
                sqsClient = new AmazonSQSClient();
            }

            InfrastructureConstants.OrderPreparingQueueUrl = sqsClient.GetQueueUrlAsync(InfrastructureConstants.OrderPreparingQueueName).GetAwaiter()
                .GetResult().QueueUrl;

            InfrastructureConstants.OrderBakedQueueUrl = sqsClient.GetQueueUrlAsync(InfrastructureConstants.OrderBakedQueueName).GetAwaiter()
                .GetResult().QueueUrl;

            InfrastructureConstants.OrderPrepCompleteQueueUrl = sqsClient.GetQueueUrlAsync(InfrastructureConstants.OrderPrepCompleteQueueName).GetAwaiter()
                .GetResult().QueueUrl;

            InfrastructureConstants.OrderQualityCheckedQueueUrl = sqsClient.GetQueueUrlAsync(InfrastructureConstants.OrderQualityCheckedQueueName).GetAwaiter()
                .GetResult().QueueUrl;

            InfrastructureConstants.DriverCollectedQueueUrl = sqsClient.GetQueueUrlAsync(InfrastructureConstants.DriverCollectedQueueName).GetAwaiter()
                .GetResult().QueueUrl;

            InfrastructureConstants.OrderDeliveredQueueUrl = sqsClient.GetQueueUrlAsync(InfrastructureConstants.OrderDeliveredQueueName).GetAwaiter()
                .GetResult().QueueUrl;
        }
    }
}