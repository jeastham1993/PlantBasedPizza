using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.Orders.IntegrationTest.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Orders.IntegrationTest.Steps;

[Binding]
public class OrderSteps
{
    private readonly OrdersTestDriver _driver;
    private readonly EventDriver _eventDriver;
    private readonly ScenarioContext _scenarioContext;

    public OrderSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _driver = new OrdersTestDriver();
        _eventDriver = new EventDriver();
    }

    [Given(@"a LoyaltyPointsUpdatedEvent is published for customer (.*), with a points total of (.*)")]
    public async Task GivenALoyaltyPointsUpdatedEventIsPublishedForCustomerWithAPointsTotalOf(string p0, decimal p1)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        await _eventDriver.SimulateLoyaltyPointsUpdatedEvent(p0, p1);
    }

    [Given(@"a new (.*) order is created")]
    public async Task GivenANewOrderIsCreatedWithIdentifierOrd(string p0)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        if (p0 == "delivery")
        {
            var order = await _driver.AddNewDeliveryOrder("james").ConfigureAwait(false);
            _scenarioContext.Add("orderId", order.OrderIdentifier);
        }
        else
        {
            var order = await _driver.AddNewPickupOrder("james").ConfigureAwait(false);
            _scenarioContext.Add("orderId", order.OrderIdentifier);
        }
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

    [When(@"order is cancelled")]
    public async Task WhenOrderIsCancelled()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var orderId = _scenarioContext.Get<string>("orderId");

        await _driver.CancelOrder(orderId);
    }

    [When(@"kitchen quality checks the order")]
    public async Task WhenKitchenQualityChecksTheOrder()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var orderId = _scenarioContext.Get<string>("orderId");

        await _eventDriver.SimulateQualityCheckCompleteEvent(orderId);
    }

    [When(@"order is delivery successfully")]
    public async Task WhenOrderIsDeliverySuccessfully()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var orderId = _scenarioContext.Get<string>("orderId");

        await _eventDriver.SimulateOrderDeliveredEvent(orderId);
    }

    [When(@"payment is successful")]
    public async Task PaymentIsSuccessful()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");

        var orderId = _scenarioContext.Get<string>("orderId");

        await _eventDriver.SimulatePaymentSuccessEvent(orderId, 100);
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
    public async Task ThenOrderHistoryShouldExistWithDescription(string p0)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        var orderId = _scenarioContext.Get<string>("orderId");

        var order = await _driver.GetOrder(orderId).ConfigureAwait(false);

        order.History.Exists(p => p.Description.Contains(p0)).Should().BeTrue($"Order {orderId} should contain a {p0} message");
    }

    [Then(@"order should not contain a (.*) event")]
    public async Task ThenOrderHistoryShouldNotExist(string p0)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        var orderId = _scenarioContext.Get<string>("orderId");

        var order = await _driver.GetOrder(orderId).ConfigureAwait(false);

        order.History.Exists(p => p.Description.Contains(p0)).Should().BeFalse($"Order {orderId} should not contain a {p0} message");
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

    [When(@"user does not cancel")]
    public async Task WhenUserDoesNotCancel()
    {
        // The user workflow gives a 10s period for a user to cancel their order, wait for that to pass
        await Task.Delay(TimeSpan.FromSeconds(15));
    }

    [Given(@"an invalid payment success event is received")]
    public async Task GivenAnInvalidPaymentSuccessIsReceived()
    {
        var eventId = Guid.NewGuid().ToString();
        _scenarioContext.Add("eventId", eventId);
        
        await _eventDriver.SimulateInvalidPaymentSuccessEvent();
    }

    [Then(@"message should arrive in dead letter inbox")]
    public async Task ThenMessageShouldArriveInDeadLetterInbox()
    {
        // Wait for retry policies
        await Task.Delay(TimeSpan.FromSeconds(3));
        
        var eventId = _scenarioContext.Get<string>("eventId");

        await _eventDriver.VerifyEventReachesDeadLetterInbox(eventId);
    }
}