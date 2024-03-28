using FluentAssertions;
using PlantBasedPizza.E2ETests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.E2ETests.Steps
{
    [Binding]
    public sealed class OrderManagerStepDefinitions
    {
        private readonly OrderManagerDriver _driver;

        public OrderManagerStepDefinitions(ScenarioContext scenarioContext)
        {
            this._driver = new OrderManagerDriver();
        }

        [Given(@"a new order is created with identifier (.*) for customer (.*)")]
        public async Task GivenANewOrderIsCreatedWithIdentifierOrd(string p0, string p1)
        {
            await this._driver.AddNewOrder(p0, p1).ConfigureAwait(false);
        }

        [When(@"a (.*) is added to order (.*)")]
        public async Task WhenAnItemIsAdded(string p0, string p1)
        {
            await this._driver.AddItemToOrder(p1, p0, 1);
        }

        [Then(@"there should be (.*) item on the order with identifier (.*)")]
        public async Task ThenThereShouldBeItemOnTheOrder(int p0, string p1)
        {
            var order = await this._driver.GetOrder(p1);

            order.Items.Count.Should().Be(p0);
        }

        [When(@"order (.*) is submitted")]
        public async Task WhenOrderOrdIsSubmitted(string p0)
        {
            await this._driver.SubmitOrder(p0);
        }

        [Then(@"order (.*) should be marked as (.*)")]
        public async Task ThenOrderOrdShouldBeMarkedAsCompleted(string p0, string p1)
        {
            var order = await this._driver.GetOrder(p0).ConfigureAwait(false);

            order.OrderCompletedOn.Should().NotBeNull();
        }

        [Then(@"order (.*) should contain a (.*) event")]
        public async Task ThenOrderOrdShouldContainAOrderQualityCheckedEvent(string p0, string p1)
        {
            var order = await this._driver.GetOrder(p0).ConfigureAwait(false);

            order.History.Exists(p => p.Description == p1).Should().BeTrue();
        }

        [Then(@"order (.*) should be awaiting collection")]
        public async Task ThenOrderOrdShouldBeAwaitingCollection(string p0)
        {
            var order = await this._driver.GetOrder(p0).ConfigureAwait(false);

            order.AwaitingCollection.Should().BeTrue();
        }

        [When(@"order (.*) is collected")]
        public async Task WhenOrderOrdIsCollected(string p0)
        {
            await this._driver.CollectOrder(p0).ConfigureAwait(false);
        }

        [Given(@"a new delivery order is created with identifier (.*) for customer (.*)")]
        public async Task GivenANewDeliveryOrderIsCreatedWithIdentifierDeliver(string p0, string p1)
        {
            await this._driver.AddNewDeliveryOrder(p0, p1);
        }
    }
}