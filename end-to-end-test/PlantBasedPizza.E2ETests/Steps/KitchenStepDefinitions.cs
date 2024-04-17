using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.E2ETests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.E2ETests.Steps
{
    [Binding]
    public sealed class KitchenStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly KitchenDriver _kitchenDriver;

        public KitchenStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            this._kitchenDriver = new KitchenDriver();
        }

        [When(@"order is processed by the kitchen")]
        public async Task WhenOrderOrdIsProcessedByTheKitchen()
        {
            Activity.Current = _scenarioContext.Get<Activity>("Activity");
            
            var orderId = _scenarioContext.Get<string>("orderId");
            
            await this._kitchenDriver.Preparing(orderId);
            await this._kitchenDriver.PrepComplete(orderId);
            await this._kitchenDriver.BakeComplete(orderId);
            await this._kitchenDriver.QualityChecked(orderId);
        }
    }
}