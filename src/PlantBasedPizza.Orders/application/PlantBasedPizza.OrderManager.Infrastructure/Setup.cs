using System.Net;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.OrderSubmitted;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Caching;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public static class Setup
{
    public static IServiceCollection AddOrderManagerInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ServiceEndpoints>(configuration.GetSection("Services"));

        var client = new MongoClient(configuration["DatabaseConnection"]);

        services.AddSingleton(client);

        services.AddCaching(configuration);

        BsonClassMap.RegisterClassMap<OutboxItem>(map =>
        {
            map.AutoMap();
            map.SetIgnoreExtraElements(true);
            map.SetIgnoreExtraElementsIsInherited(true);
        });

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

        services.AddHttpClient<RecipeService>();

        services.AddGrpcClient<Loyalty.LoyaltyClient>(o =>
            {
                o.Address = new Uri(configuration["Services:LoyaltyInternal"]);
            })
            .ConfigureChannel((provider, channel) =>
            {
                channel.ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } };
            });

        services.AddCaching(configuration);

        services.AddSingleton<IOrderRepository, OrderRepository>();
        services.AddSingleton<CollectOrderCommandHandler>();
        services.AddSingleton<AddItemToOrderHandler>();
        services.AddSingleton<CreateDeliveryOrderCommandHandler>();
        services.AddSingleton<CreatePickupOrderCommandHandler>();
        services.AddSingleton<IRecipeService, RecipeService>();
        services.AddSingleton<ILoyaltyPointService, LoyaltyPointService>();
        services.AddSingleton<OrderManagerHealthChecks>();
        services.AddSingleton<IOrderEventPublisher, OrderEventPublisher>();
        services.AddSingleton<SubmitOrderCommandHandler>();

        services.AddHttpClient("retry-http-client")
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

        services.AddLogging();

        services.AddDaprClient();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(delay);
    }
}