using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.Kitchen.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Kitchen.IntegrationTests.Steps;

[Binding]
public class KitchenSteps
{
    private readonly KitchenDriver _kitchenDriver;
    private readonly ScenarioContext _scenarioContext;

    public KitchenSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _kitchenDriver = new KitchenDriver();
    }

    [Given(@"a new order submitted event is raised")]
    public async Task GivenANewOrderSubmittedEventIsRaisedForOrderOrd()
    {
        var orderId = Guid.NewGuid().ToString();

        _scenarioContext.Add("orderId", orderId);

        await _kitchenDriver.NewOrderSubmitted(orderId);
    }

    [Given(@"a new order submitted event is raised twice")]
    public async Task GivenANewOrderSubmittedEventIsRaisedTwice()
    {
        var orderId = Guid.NewGuid().ToString();

        _scenarioContext.Add("orderId", orderId);
        
        var eventId = Guid.NewGuid().ToString();

        await _kitchenDriver.NewOrderSubmitted(orderId, Guid.NewGuid().ToString());
        await _kitchenDriver.NewOrderSubmitted(orderId, Guid.NewGuid().ToString());
    }

    [When(@"order is processed by the kitchen")]
    public async Task WhenOrderOrdIsProcessedByTheKitchen()
    {
        var orderId = _scenarioContext.Get<string>("orderId");
        
        await _kitchenDriver.Preparing(orderId);
        await _kitchenDriver.PrepComplete(orderId);
        await _kitchenDriver.BakeComplete(orderId);
        await _kitchenDriver.QualityChecked(orderId);
    }

    [When(@"order is marked as preparing")]
    public async Task WhenOrderOrdIsMarkedAsPreparing()
    {
        var orderId = _scenarioContext.Get<string>("orderId");

        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        await _kitchenDriver.Preparing(orderId);
    }

    [When(@"order is marked as prep-complete")]
    public async Task WhenOrderOrdIsMarkedAsPrepComplete()
    {
        var orderId = _scenarioContext.Get<string>("orderId");
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        await _kitchenDriver.PrepComplete(orderId);
    }

    [When(@"order is marked as bake-complete")]
    public async Task WhenOrderOrdIsMarkedAsBakeComplete()
    {
        var orderId = _scenarioContext.Get<string>("orderId");
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        await _kitchenDriver.BakeComplete(orderId);
    }

    [When(@"order is marked as quality-checked")]
    public async Task WhenOrderOrdIsMarkedAsQualityChecked()
    {
        var orderId = _scenarioContext.Get<string>("orderId");
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        
        await _kitchenDriver.QualityChecked(orderId);
    }

    [Then(@"order should appear in the preparing queue")]
    public async Task ThenOrderOrdShouldAppearInThePreparingQueue()
    {
        var orderId = _scenarioContext.Get<string>("orderId");
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        
        var requests = await _kitchenDriver.GetPreparing();
        requests.Exists(p => p.OrderIdentifier == orderId).Should().BeTrue();
    }

    [Then(@"order should appear as new")]
    public async Task ThenOrderShouldAppearAsNew()
    {
        var orderId = _scenarioContext.Get<string>("orderId");
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        
        var requests = await _kitchenDriver.GetNew();

        requests.Exists(p => p.OrderIdentifier == orderId).Should().BeTrue();
    }

    [Then(@"order should appear as new once")]
    public async Task ThenOrderShouldAppearAsNewOnce()
    {
        var orderId = _scenarioContext.Get<string>("orderId");
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        
        var requests = await _kitchenDriver.GetNew();

        requests.Count(p => p.OrderIdentifier == orderId).Should().Be(1);
    }

    [Then(@"order should appear in the baking queue")]
    public async Task ThenOrderOrdShouldAppearInTheBakingQueueQueue()
    {
        var orderId = _scenarioContext.Get<string>("orderId");
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        
        var requests = await _kitchenDriver.GetBaking();

        requests.Exists(p => p.OrderIdentifier == orderId).Should().BeTrue();
    }

    [Then(@"order should appear in the quality check queue")]
    public async Task ThenOrderOrdShouldAppearInTheQualityCheckQueue()
    {
        var orderId = _scenarioContext.Get<string>("orderId");
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        
        var requests = await _kitchenDriver.GetQualityChecked();
        requests.Exists(p => p.OrderIdentifier == orderId).Should().BeTrue();
    }
}