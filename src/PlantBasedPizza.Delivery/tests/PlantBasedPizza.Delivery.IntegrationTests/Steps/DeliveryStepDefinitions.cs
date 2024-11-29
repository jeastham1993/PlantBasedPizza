using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.Delivery.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Delivery.IntegrationTests.Steps
{
    [Binding]
    public sealed class DeliveryStepDefinitions
    {
        private readonly DeliveryDriver _driver;
        private readonly ScenarioContext _scenarioContext;

        public DeliveryStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _driver = new DeliveryDriver();
        }

        [Given(@"an order is ready for delivery")]
        public async Task ANewOrderIsReadyForDelivery()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");

            var orderId = Guid.NewGuid().ToString();
            _scenarioContext.Add("orderId", orderId);

            await _driver.ANewOrderIsReadyForDelivery(orderId);
        }

        [Given(@"an order is ready for delivery twice")]
        public async Task ANewOrderIsReadyForDeliveryTwice()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");

            var orderId = Guid.NewGuid().ToString();
            _scenarioContext.Add("orderId", orderId);
            
            var eventId = Guid.NewGuid().ToString();

            await _driver.ANewOrderIsReadyForDelivery(orderId, eventId);
            await _driver.ANewOrderIsReadyForDelivery(orderId, eventId);
        }

        [Then(@"it should be awaiting delivery collection once")]
        public async Task ThenOrderShouldBeAwaitingDeliveryOnce()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var ordersAwaitingDriver = await _driver.GetAwaitingDriver();

            ordersAwaitingDriver.Count(p => p.OrderIdentifier == orderId).Should().Be(1);
            
        }

        [Then(@"it should be awaiting delivery collection")]
        public async Task ThenOrderDeliverShouldBeAwaitingDeliveryCollection()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var ordersAwaitingDriver = await _driver.GetAwaitingDriver();

            ordersAwaitingDriver.Exists(p => p.OrderIdentifier == orderId).Should().BeTrue();
        }

        [When(@"it is assigned to a driver named (.*)")]
        public async Task WhenOrderDeliverIsAssignedToADriverNamedJames(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            await _driver.AssignDriver(orderId, p0);
        }

        [Then(@"it should appear in a list of (.*) deliveries")]
        public async Task ThenOrderDeliverShouldAppearInAListOfJamesDeliveries(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var ordersForDriver = await _driver.GetAssignedDeliveriesForDriver(p0);

            ordersForDriver.Exists(p => p.OrderIdentifier == orderId).Should().BeTrue();
        }

        [When(@"it is delivered")]
        public async Task WhenOrderDeliverIsDelivered()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            await _driver.DeliverOrder(orderId);
        }

        [Then(@"it should no longer be assigned to a driver named (.*)")]
        public async Task ThenOrderDeliverShouldNoLongerBeAssignedToADriverNamedJames(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var ordersForDriver = await _driver.GetAssignedDeliveriesForDriver(p0);

            ordersForDriver.Exists(p => p.OrderIdentifier == orderId).Should().BeFalse();
        }
    }
}