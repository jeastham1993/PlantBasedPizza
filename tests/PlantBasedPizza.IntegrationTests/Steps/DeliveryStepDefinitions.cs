using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using PlantBasedPizza.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.IntegrationTests.Steps
{
    [Binding]
    public sealed class DeliveryStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly DeliveryDriver _driver;

        public DeliveryStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            this._driver = new DeliveryDriver();
        }

        [Then(@"order (.*) should be awaiting delivery collection")]
        public async Task ThenOrderDeliverShouldBeAwaitingDeliveryCollection(string p0)
        {
            var ordersAwaitingDriver = await this._driver.GetAwaitingDriver();

            ordersAwaitingDriver.Any(p => p.OrderIdentifier == p0).Should().BeTrue();
        }

        [When(@"order (.*) is assigned to a driver named (.*)")]
        public async Task WhenOrderDeliverIsAssignedToADriverNamedJames(string p0, string p1)
        {
            await this._driver.AssignDriver(p0, p1);
        }

        [Then(@"order (.*) should appear in a list of (.*) deliveries")]
        public async Task ThenOrderDeliverShouldAppearInAListOfJamesDeliveries(string p0, string p1)
        {
            var ordersForDriver = await this._driver.GetAssignedDeliveriesForDriver(p1);

            ordersForDriver.Any(p => p.OrderIdentifier == p0).Should().BeTrue();
        }

        [When(@"order (.*) is delivered")]
        public async Task WhenOrderDeliverIsDelivered(string p0)
        {
            await this._driver.DeliverOrder(p0);
        }

        [Then(@"order (.*) should no longer be assigned to a driver named (.*)")]
        public async Task ThenOrderDeliverShouldNoLongerBeAssignedToADriverNamedJames(string p0, string p1)
        {
            var ordersForDriver = await this._driver.GetAssignedDeliveriesForDriver(p1);

            ordersForDriver.Any(p => p.OrderIdentifier == p0).Should().BeFalse();
        }
    }
}