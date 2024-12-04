using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Shared.Caching;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    using MongoDB.Bson.Serialization;

    public static class Setup
    {
        public static IServiceCollection AddKitchenInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            var client = new MongoClient(configuration["DatabaseConnection"]);

            services.AddSingleton(client);
            services.Configure<ServiceEndpoints>(configuration.GetSection("Services"));
            services.AddDaprClient();
            services.AddCaching(configuration);
            
            BsonClassMap.RegisterClassMap<OutboxItem>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            BsonClassMap.RegisterClassMap<KitchenRequest>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            services.AddHttpClient("retry-http-client")
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());
            
            // Add default gRPC retries
            var defaultMethodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            };
            
            services.AddGrpcClient<Orders.Internal.Orders.OrdersClient>(o =>
                {
                    o.Address = new Uri(configuration["Services:OrdersInternal"]);
                })
                .ConfigureChannel((provider, channel) =>
                {
                    channel.ServiceConfig = new ServiceConfig() { MethodConfigs = { defaultMethodConfig } };
                });
            
            services.AddSingleton<IRecipeService, RecipeService>();
            services.AddSingleton<IOrderService, OrderService>();
            services.AddSingleton<IKitchenRequestRepository, KitchenRequestRepository>();
            services.AddSingleton<IDeadLetterRepository, DeadLetterRepository>();
            services.AddSingleton<IKitchenEventPublisher, KitchenEventPublisher>();

            return services;
        }
        
        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);
            
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(delay);
        }
    }
}