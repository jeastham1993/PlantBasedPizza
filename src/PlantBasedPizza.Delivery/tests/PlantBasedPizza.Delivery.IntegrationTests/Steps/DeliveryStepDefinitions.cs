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
            this._driver = new DeliveryDriver();
        }

        [Given(@"an order is ready for delivery")]
        public async Task ANewOrderIsReadyForDelivery()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");

            var orderId = Guid.NewGuid().ToString();
            _scenarioContext.Add("orderId", orderId);

            await this._driver.ANewOrderIsReadyForDelivery(orderId);
        }

        [Then(@"it should be awaiting delivery collection")]
        public async Task ThenOrderDeliverShouldBeAwaitingDeliveryCollection()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var ordersAwaitingDriver = await this._driver.GetAwaitingDriver();

            ordersAwaitingDriver.Exists(p => p.OrderIdentifier == orderId).Should().BeTrue();
        }

        [When(@"it is assigned to a driver named (.*)")]
        public async Task WhenOrderDeliverIsAssignedToADriverNamedJames(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            await this._driver.AssignDriver(orderId, p0);
        }

        [Then(@"it should appear in a list of (.*) deliveries")]
        public async Task ThenOrderDeliverShouldAppearInAListOfJamesDeliveries(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var ordersForDriver = await this._driver.GetAssignedDeliveriesForDriver(p0);

            ordersForDriver.Exists(p => p.OrderIdentifier == orderId).Should().BeTrue();
        }

        [When(@"it is delivered")]
        public async Task WhenOrderDeliverIsDelivered()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            await this._driver.DeliverOrder(orderId);
        }

        [Then(@"it should no longer be assigned to a driver named (.*)")]
        public async Task ThenOrderDeliverShouldNoLongerBeAssignedToADriverNamedJames(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var ordersForDriver = await this._driver.GetAssignedDeliveriesForDriver(p0);

            ordersForDriver.Exists(p => p.OrderIdentifier == orderId).Should().BeFalse();
        }
    }
}