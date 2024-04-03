using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Handlers;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.ServiceDiscovery;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    using MongoDB.Bson.Serialization;

    public static class Setup
    {
        public static IServiceCollection AddOrderManagerInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            BsonClassMap.RegisterClassMap<Order>(map =>
            {
                map.AutoMap();
                map.MapField("_items");
                map.MapField("_history");
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            BsonClassMap.RegisterClassMap<OrderItem>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
            BsonClassMap.RegisterClassMap<DeliveryDetails>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
            
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
            
            services.AddGrpcClient<Loyalty.LoyaltyClient>(o =>
            {
                o.Address = new Uri(configuration["Services:LoyaltyInternal"]);
            })
            .ConfigureChannel((provider, channel) =>
            {
                channel.HttpHandler = provider.GetRequiredService<ServiceRegistryHttpMessageHandler>();
                channel.ServiceConfig = new ServiceConfig() { MethodConfigs = { defaultMethodConfig } };
            });
            
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddSingleton<CollectOrderCommandHandler>();
            services.AddSingleton<AddItemToOrderHandler>();
            services.AddSingleton<CreateDeliveryOrderCommandHandler>();
            services.AddSingleton<CreatePickupOrderCommandHandler>();
            services.AddSingleton<IRecipeService, RecipeService>();
            services.AddSingleton<ILoyaltyPointService, LoyaltyPointService>();
            
            services.AddSingleton<Handles<OrderPreparingEvent>, OrderPreparingEventHandler>();
            services.AddSingleton<Handles<OrderPrepCompleteEvent>, OrderPrepCompleteEventHandler>();
            services.AddSingleton<Handles<OrderBakedEvent>, OrderBakedEventHandler>();
            services.AddSingleton<Handles<OrderQualityCheckedEvent>, OrderQualityCheckedEventHandler>();
            services.AddSingleton<Handles<OrderDeliveredEvent>, DriverDeliveredOrderEventHandler>();
            services.AddSingleton<Handles<DriverCollectedOrderEvent>, DriverCollectedOrderEventHandler>();

            services.AddSingleton<OrderManagerHealthChecks>();
            services.AddHttpClient<OrderManagerHealthChecks>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(configuration["Services:Loyalty"]))
                .AddHttpMessageHandler<ServiceRegistryHttpMessageHandler>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());
            
            services.AddLogging();

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