using Dapr.Client;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Payments.ExternalEvents;

namespace PlantBasedPizza.Payments.IntegrationTests.Drivers;

public class PaymentDriver
    {
        private readonly DaprClient _daprClient;
        private readonly IDistributedCache _distributedCache;

        public PaymentDriver()
        {
            _daprClient = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:5101")
                .Build();
            _distributedCache = new RedisCache(Options.Create<RedisCacheOptions>(new RedisCacheOptions() { Configuration = "localhost:6379", InstanceName = "payments" }));
        }

        public async Task SimulateOrderSubmittedEvent(string orderIdentifier)
        {
            await _daprClient.PublishEventAsync("public", "order.orderSubmitted.v1", new OrderSubmittedEventV1()
            {
                OrderIdentifier = orderIdentifier
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        public async Task<string?> GetCachedPaymentStatus(string orderIdentifier)
        {
            var cachedValue = await _distributedCache.GetStringAsync(orderIdentifier);
            
            return cachedValue;
        }
    }