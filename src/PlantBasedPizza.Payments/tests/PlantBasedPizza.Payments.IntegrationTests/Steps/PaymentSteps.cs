using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.Payments.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Payments.IntegrationTests.Steps;

[Binding]
public sealed class PaymentSteps
{
    private readonly PaymentDriver _driver;
    private readonly ScenarioContext _scenarioContext;

    public PaymentSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _driver = new PaymentDriver();
    }

    [When(@"an order submitted event is received")]
    public async Task ThenAOrderSubmittedEventIsHandled()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        
        var orderIdentifier = Guid.NewGuid().ToString();
        _scenarioContext.Add("orderId", orderIdentifier);
        
        await _driver.SimulateOrderSubmittedEvent(orderIdentifier);
    }

    [Then("the payment should be processed and cached")]
    public async Task ThenThePaymentShouldBeProcessedAndCached()
    {
        var orderId = _scenarioContext.Get<string>("orderId");
        
        var paymentStatus = await _driver.GetCachedPaymentStatus(orderId);
        paymentStatus.Should().NotBeNull();
        paymentStatus.Should().Be("processed");
    }
}