using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.Orders.IntegrationTest.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Orders.IntegrationTest.Steps;

[Binding]
public class OrderSteps
{
    private readonly OrdersTestDriver _driver;
    private readonly ScenarioContext _scenarioContext;

    public OrderSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _driver = new OrdersTestDriver();
    }

    [Given(@"a LoyaltyPointsUpdatedEvent is published for customer (.*), with a points total of (.*)")]
    public async Task GivenALoyaltyPointsUpdatedEventIsPublishedForCustomerWithAPointsTotalOf(string p0, decimal p1)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        await _driver.SimulateLoyaltyPointsUpdatedEvent(p0, p1);

        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    [Given(@"a new order is created")]
    public async Task GivenANewOrderIsCreatedWithIdentifierOrd()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var order = await _driver.AddNewOrder("james").ConfigureAwait(false);
        _scenarioContext.Add("orderId", order.OrderIdentifier);
    }

    [When(@"a (.*) is added to order")]
    public async Task WhenAnItemIsAdded(string p0)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.AddItemToOrder(orderId, p0, 1);
    }

    [When(@"order is submitted")]
    public async Task WhenOrderOrdIsSubmitted()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.SubmitOrder(orderId);
    }

    [When(@"payment is successful")]
    public async Task PaymentIsSuccessful()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.SimulatePaymentSuccessEvent(orderId, 100);
    }

    [Then(@"order should be marked as (.*)")]
    public async Task ThenOrderOrdShouldBeMarkedAsCompleted(string p0)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var orderId = _scenarioContext.Get<string>("orderId");

        var order = await _driver.GetOrder(orderId).ConfigureAwait(false);

        order.OrderCompletedOn.Should().NotBeNull();
    }

    [Then(@"order should contain a (.*) event")]
    public async Task ThenOrderOrdShouldContainAOrderQualityCheckedEvent(string p0)
    {
        // Allow async processes to catch up
        await Task.Delay(TimeSpan.FromSeconds(10));

        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        var orderId = _scenarioContext.Get<string>("orderId");

        var order = await _driver.GetOrder(orderId).ConfigureAwait(false);

        order.History.Exists(p => p.Description.Contains(p0)).Should().BeTrue();
    }

    [Then(@"order should be awaiting collection")]
    public async Task ThenOrderOrdShouldBeAwaitingCollection()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var orderId = _scenarioContext.Get<string>("orderId");

        var order = await _driver.GetOrder(orderId).ConfigureAwait(false);

        order.AwaitingCollection.Should().BeTrue();
    }

    [When(@"order is collected")]
    public async Task WhenOrderOrdIsCollected()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.CollectOrder(orderId).ConfigureAwait(false);
    }

    [Given(@"a new delivery order is created for customer (.*)")]
    public async Task GivenANewDeliveryOrderIsCreatedWithIdentifierDeliver(string p0)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var order = await _driver.AddNewDeliveryOrder(p0);

        _scenarioContext.Add("orderId", order.OrderIdentifier);
    }
}