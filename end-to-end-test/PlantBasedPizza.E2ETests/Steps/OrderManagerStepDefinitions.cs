using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.E2ETests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.E2ETests.Steps
{
    [Binding]
    public sealed class OrderManagerStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly OrderManagerDriver _driver;

        public OrderManagerStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _driver = new OrderManagerDriver();
        }

        [Given(@"a new order is created for customer (.*)")]
        public async Task GivenANewOrderIsCreatedWithIdentifierOrd(string p0)
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");

            var orderId = Guid.NewGuid().ToString();
            _scenarioContext.Add("orderId", orderId);
            
            await _driver.AddNewOrder(orderId, p0).ConfigureAwait(false);
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

            order.History.Exists(p => p.Description == p0).Should().BeTrue();
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

            var orderId = Guid.NewGuid().ToString();
            _scenarioContext.Add("orderId", orderId);
            
            await _driver.AddNewDeliveryOrder(orderId, p0);
        }
    }
}