using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Orders.IntegrationTest.Drivers;
using StackExchange.Redis;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Orders.IntegrationTest.Steps;

[Binding]
public partial class OrderSteps
{
    private readonly OrdersTestDriver _driver;
    private readonly ScenarioContext _scenarioContext;
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    private readonly IDistributedCache _distributedCache;
    
    public OrderSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _driver = new OrdersTestDriver();
        _connectionMultiplexer = ConnectionMultiplexer.Connect("localhost");
        _connectionMultiplexer.GetDatabase();
        _distributedCache = new RedisCache(Options.Create(new RedisCacheOptions()
        {
            InstanceName = "Orders",
            Configuration = "localhost:6379"
        }));
    }
    
    [Given(@"a LoyaltyPointsUpdatedEvent is published for customer (.*), with a points total of (.*)")]
    public async Task GivenALoyaltyPointsUpdatedEventIsPublishedForCustomerWithAPointsTotalOf(string p0, decimal p1)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        await this._driver.SimulateLoyaltyPointsUpdatedEvent(p0, p1);

        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    [Then(@"loyalty points should be cached for (.*) with a total amount of (.*)")]
    public async Task ThenLoyaltyPointsShouldBeCachedWithATotalAmount(string p0, decimal p1)
    {
        var pointsTotal = await _distributedCache.GetStringAsync(p0.ToUpper());

        pointsTotal.Should().Be(p1.ToString());
    }
    
    [Given(@"a new order is created")]
        public async Task GivenANewOrderIsCreatedWithIdentifierOrd()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");

            var orderId = Guid.NewGuid().ToString();
            _scenarioContext.Add("orderId", orderId);
            
            await this._driver.AddNewOrder(orderId, "james").ConfigureAwait(false);
        }

        [When(@"a (.*) is added to order")]
        public async Task WhenAnItemIsAdded(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            var orderId = _scenarioContext.Get<string>("orderId");
            
            await this._driver.AddItemToOrder(orderId, p0, 1);
        }

        [When(@"order is submitted")]
        public async Task WhenOrderOrdIsSubmitted()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            await this._driver.SubmitOrder(orderId);
        }

        [Then(@"order should be marked as (.*)")]
        public async Task ThenOrderOrdShouldBeMarkedAsCompleted(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var order = await this._driver.GetOrder(orderId).ConfigureAwait(false);

            order.OrderCompletedOn.Should().NotBeNull();
        }

        [Then(@"order should contain a (.*) event")]
        public async Task ThenOrderOrdShouldContainAOrderQualityCheckedEvent(string p0)
        {
            // Allow async processes to catch up
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var order = await this._driver.GetOrder(orderId).ConfigureAwait(false);

            order.History.Exists(p => p.Description == p0).Should().BeTrue();
        }

        [Then(@"order should be awaiting collection")]
        public async Task ThenOrderOrdShouldBeAwaitingCollection()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var order = await this._driver.GetOrder(orderId).ConfigureAwait(false);

            order.AwaitingCollection.Should().BeTrue();
        }

        [When(@"order is collected")]
        public async Task WhenOrderOrdIsCollected()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            await this._driver.CollectOrder(orderId).ConfigureAwait(false);
        }

        [Given(@"a new delivery order is created for customer (.*)")]
        public async Task GivenANewDeliveryOrderIsCreatedWithIdentifierDeliver(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");

            var orderId = Guid.NewGuid().ToString();
            _scenarioContext.Add("orderId", orderId);
            
            await this._driver.AddNewDeliveryOrder(orderId, p0);
        }
}