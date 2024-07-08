using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddOrderManagerInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<ServiceEndpoints>(configuration.GetSection("Services"));

            if (!string.IsNullOrEmpty(configuration["RedisConnectionString"]))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration["RedisConnectionString"];
                    options.InstanceName = "Orders";
                });   
            }
            else
            {
                services.AddDistributedMemoryCache();
            }
            
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddSingleton<CollectOrderCommandHandler>();
            services.AddSingleton<AddItemToOrderHandler>();
            services.AddSingleton<CreateDeliveryOrderCommandHandler>();
            services.AddSingleton<CreatePickupOrderCommandHandler>();
            services.AddSingleton<IRecipeService, RecipeService>(); ;
            services.AddSingleton<OrderManagerHealthChecks>();
            services.AddSingleton<IOrderEventPublisher, OrderEventPublisher>();

            services.AddSingleton<AmazonDynamoDBClient>();
            services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));
            
            services.AddHttpClient("recipe-service")
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