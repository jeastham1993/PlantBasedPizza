﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using PlantBasedPizza.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.IntegrationTests.Steps
{
    [Binding]
    public sealed class KitchenStepDefinitions
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef
        private readonly ScenarioContext _scenarioContext;
        private readonly KitchenDriver _kitchenDriver;

        public KitchenStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            this._kitchenDriver = new KitchenDriver();
        }

        [Then(@"an order with identifier (.*) should be added to the new kitchen requests")]
        public async Task ThenAnOrderWithIdentifierOrdShouldBeAddedToTheNewKitchenRequests(string p0)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var newKitchenRequests = await this._kitchenDriver.GetNew();

            newKitchenRequests.Any(p => p.OrderIdentifier == p0).Should().BeTrue();
        }

        [When(@"order (.*) is processed by the kitchen")]
        public async Task WhenOrderOrdIsProcessedByTheKitchen(string p0)
        {
            await this._kitchenDriver.Preparing(p0);
            await this._kitchenDriver.PrepComplete(p0);
            await this._kitchenDriver.BakeComplete(p0);
            await this._kitchenDriver.QualityChecked(p0);
        }

        [When(@"order (.*) is marked as preparing")]
        public async Task WhenOrderOrdIsMarkedAsPreparing(string p0)
        {
            await this._kitchenDriver.Preparing(p0);
        }

        [When(@"order (.*) is marked as prep-complete")]
        public async Task WhenOrderOrdIsMarkedAsPrepComplete(string p0)
        {
            await this._kitchenDriver.PrepComplete(p0);
        }
        
        [When(@"order (.*) is marked as bake-complete")]
        public async Task WhenOrderOrdIsMarkedAsBakeComplete(string p0)
        {
            await this._kitchenDriver.BakeComplete(p0);
        }
        
        [When(@"order (.*) is marked as quality-checked")]
        public async Task WhenOrderOrdIsMarkedAsQualityChecked(string p0)
        {
            await this._kitchenDriver.QualityChecked(p0);
        }

        [Then(@"order (.*) should appear in the preparing queue")]
        public async Task ThenOrderOrdShouldAppearInThePreparingQueue(string p0)
        {
            var requests = await this._kitchenDriver.GetPreparing();

            requests.Any(p => p.OrderIdentifier == p0).Should().BeTrue();
        }

        [Then(@"order (.*) should appear in the baking queue")]
        public async Task ThenOrderOrdShouldAppearInTheBakingQueueQueue(string p0)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var requests = await this._kitchenDriver.GetBaking();

            requests.Any(p => p.OrderIdentifier == p0).Should().BeTrue();
        }

        [Then(@"order (.*) should appear in the quality check queue")]
        public async Task ThenOrderOrdShouldAppearInTheQualityCheckQueue(string p0)
        {
            var requests = await this._kitchenDriver.GetQualityChecked();

            requests.Any(p => p.OrderIdentifier == p0).Should().BeTrue();
        }
    }
}