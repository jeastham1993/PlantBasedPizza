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
        _distributedCache = new RedisCache(Options.Create<RedisCacheOptions>(new RedisCacheOptions()
        {
            InstanceName = "OrdersWorker",
            Configuration = "localhost"
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
    public async Task ThenLoyaltyPointsShouldBeCachedForJamesWithATotalAmountOf(string p0, decimal p1)
    {
        var pointsTotal = await _distributedCache.GetStringAsync(p0);

        pointsTotal.Should().Be(p1.ToString());

    }
}