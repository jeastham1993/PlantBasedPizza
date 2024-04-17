using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.E2ETests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.E2ETests.Steps
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

        [Then(@"order should be awaiting delivery collection")]
        public async Task ThenOrderDeliverShouldBeAwaitingDeliveryCollection()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var ordersAwaitingDriver = await this._driver.GetAwaitingDriver();

            ordersAwaitingDriver.Exists(p => p.OrderIdentifier == orderId).Should().BeTrue();
        }

        [When(@"order is assigned to a driver named (.*)")]
        public async Task WhenOrderDeliverIsAssignedToADriverNamedJames(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            var orderId = _scenarioContext.Get<string>("orderId");
            
            await this._driver.AssignDriver(orderId, p0);
        }

        [Then(@"order should appear in a list of (.*) deliveries")]
        public async Task ThenOrderDeliverShouldAppearInAListOfJamesDeliveries(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var orderId = _scenarioContext.Get<string>("orderId");
            var ordersForDriver = await this._driver.GetAssignedDeliveriesForDriver(p0);

            ordersForDriver.Exists(p => p.OrderIdentifier == orderId).Should().BeTrue();
        }

        [When(@"order is delivered")]
        public async Task WhenOrderDeliverIsDelivered()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            await this._driver.DeliverOrder(orderId);
        }

        [Then(@"order should no longer be assigned to a driver named (.*)")]
        public async Task ThenOrderDeliverShouldNoLongerBeAssignedToADriverNamedJames(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            var ordersForDriver = await this._driver.GetAssignedDeliveriesForDriver(p0);

            ordersForDriver.Exists(p => p.OrderIdentifier == orderId).Should().BeFalse();
        }
    }
}