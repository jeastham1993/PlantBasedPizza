using System;
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
            await Task.Delay(TimeSpan.FromSeconds(5));

            var retries = 10;

            var foundRequests = 0;

            while (retries > 0)
            {
                var requests = await this._driver.GetAwaitingDriver();

                foundRequests = requests.Count(p => p.OrderIdentifier == p0);

                if (foundRequests == 0)
                {
                    retries--;
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
                else
                {
                    break;
                }
            }

            foundRequests.Should().Be(1);
        }

        [When(@"order (.*) is assigned to a driver named (.*)")]
        public async Task WhenOrderDeliverIsAssignedToADriverNamedJames(string p0, string p1)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            await this._driver.AssignDriver(p0, p1);
        }

        [Then(@"order (.*) should appear in a list of (.*) deliveries")]
        public async Task ThenOrderDeliverShouldAppearInAListOfJamesDeliveries(string p0, string p1)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            var retries = 10;

            var foundRequests = 0;

            while (retries > 0)
            {
                var requests = await this._driver.GetAssignedDeliveriesForDriver(p1);

                foundRequests = requests.Count(p => p.OrderIdentifier == p0);

                if (foundRequests == 0)
                {
                    retries--;
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
                else
                {
                    break;
                }
            }

            foundRequests.Should().Be(1);
        }

        [When(@"order (.*) is delivered")]
        public async Task WhenOrderDeliverIsDelivered(string p0)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            await this._driver.DeliverOrder(p0);
        }

        [Then(@"order (.*) should no longer be assigned to a driver named (.*)")]
        public async Task ThenOrderDeliverShouldNoLongerBeAssignedToADriverNamedJames(string p0, string p1)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            var retries = 10;

            var foundRequests = 0;

            while (retries > 0)
            {
                var requests = await this._driver.GetAssignedDeliveriesForDriver(p1);

                foundRequests = requests.Count(p => p.OrderIdentifier == p0);

                if (foundRequests == 0)
                {
                    retries--;
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
                else
                {
                    break;
                }
            }

            foundRequests.Should().Be(1);
        }
    }
}