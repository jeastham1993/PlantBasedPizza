using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Orders.IntegrationTest.Drivers;
using StackExchange.Redis;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Orders.IntegrationTest.Steps;

[Binding]
public class OrderSteps
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
        _distributedCache = new RedisCache(Options.Create(new RedisCacheOptions
        {
            InstanceName = "Orders",
            Configuration = "localhost:6379"
        }));
    }

    [Given(@"a LoyaltyPointsUpdatedEvent is published for customer (.*), with a points total of (.*)")]
    public async Task GivenALoyaltyPointsUpdatedEventIsPublishedForCustomerWithAPointsTotalOf(string p0, decimal p1)
    {
        

        await _driver.SimulateLoyaltyPointsUpdatedEvent(p0, p1);
    }

    [Given("an order is created and submitted")]
    public async Task GivenAnOrderIsCreatedAndSubmitted()
    {
        var orderId = await _driver.AddNewOrder("james").ConfigureAwait(false);
        _scenarioContext.Add("orderId", orderId);
        
        await _driver.AddItemToOrder(orderId, "marg", 1);
        await _driver.SubmitOrder(orderId);
    }

    [When("the payment is successful")]
    public async Task WhenThePaymentIsSuccessful()
    {
        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.SimulatePaymentSuccessEvent("user-account", orderId);
    }

    [When(@"a OrderPreparingEvent is published")]
    public async Task GivenAnOrderPreparingEventIsPublished()
    {
        
        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.SimulateOrderPreparingEvent("kitchen-id", orderId);
    }

    [When(@"a OrderBakedEvent is published")]
    public async Task GivenAnOrderBakedEventEventIsPublished()
    {
        
        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.SimulateOrderBakedEvent("kitchen-id", orderId);
    }

    [When(@"a OrderPrepCompleteEvent is published")]
    public async Task GivenAnOrderPrepCompleteEventIsPublished()
    {
        
        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.SimulateOrderPrepCompleteEvent("kitchen-id", orderId);
    }

    [When(@"a OrderQualityCheckedEvent is published")]
    public async Task GivenAnOrderQualityCheckedEventIsPublished()
    {
        
        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.SimulateOrderQualityCheckedEvent("kitchen-id", orderId);
    }

    [When(@"a DriverDeliveredOrderEvent is published")]
    public async Task GivenADriverDeliveredOrderEventIsPublished()
    {
        
        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.SimulateDriverDeliveredEvent("kitchen-id", orderId);
    }

    [When(@"a DriverCollectedOrderEvent is published")]
    public async Task GivenADriverCollectedOrderEventIsPublished()
    {
        
        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.SimulateDriverCollectedEvent("kitchen-id", orderId);
    }

    [Then(@"loyalty points should be cached for (.*) with a total amount of (.*)")]
    public async Task ThenLoyaltyPointsShouldBeCachedWithATotalAmount(string p0, decimal p1)
    {
        var retries = 2;

        while (retries > 0)
        {
            try
            {
                var pointsTotal = await _distributedCache.GetStringAsync(p0.ToUpper());

                pointsTotal.Should().Be(p1.ToString());
                break;
            }
            catch (Exception)
            {
                retries--;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    [Given(@"a new order is created")]
    public async Task GivenANewOrderIsCreatedWithIdentifierOrd()
    {
        var orderId = await _driver.AddNewOrder("james").ConfigureAwait(false);
        _scenarioContext.Add("orderId", orderId);
    }

    [When(@"a (.*) is added to order")]
    public async Task WhenAnItemIsAdded(string p0)
    {
        
        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.AddItemToOrder(orderId, p0, 1);
    }

    [When(@"order is submitted")]
    public async Task WhenOrderOrdIsSubmitted()
    {
        

        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.SubmitOrder(orderId);
    }

    [Then(@"order should be marked as (.*)")]
    public async Task ThenOrderOrdShouldBeMarkedAsCompleted(string p0)
    {
        

        var orderId = _scenarioContext.Get<string>("orderId");

        var order = await _driver.GetOrder(orderId).ConfigureAwait(false);

        order.OrderCompletedOn.Should().NotBeNull();
    }

    [Then(@"order should contain a (.*) event")]
    public async Task ThenOrderOrdShouldContainAOrderQualityCheckedEvent(string p0)
    {
        // Allow async processes to catch up
        await Task.Delay(TimeSpan.FromSeconds(10));

        var orderId = _scenarioContext.Get<string>("orderId");

        var order = await _driver.GetOrder(orderId).ConfigureAwait(false);

        order.History.Exists(p => p.Description == p0).Should().BeTrue($"Order should contain a {p0} event");
    }

    [Then(@"order should be awaiting collection")]
    public async Task ThenOrderOrdShouldBeAwaitingCollection()
    {
        var orderId = _scenarioContext.Get<string>("orderId");

        var order = await _driver.GetOrder(orderId).ConfigureAwait(false);

        order.AwaitingCollection.Should().BeTrue();
    }

    [When(@"order is collected")]
    public async Task WhenOrderOrdIsCollected()
    {
        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.CollectOrder(orderId).ConfigureAwait(false);
    }

    [Given(@"a new delivery order is created for customer (.*)")]
    public async Task GivenANewDeliveryOrderIsCreatedWithIdentifierDeliver(string p0)
    {
        var orderId = await _driver.AddNewOrder("james").ConfigureAwait(false);
        _scenarioContext.Add("orderId", orderId);

        await _driver.AddNewDeliveryOrder(orderId, p0);
    }
}