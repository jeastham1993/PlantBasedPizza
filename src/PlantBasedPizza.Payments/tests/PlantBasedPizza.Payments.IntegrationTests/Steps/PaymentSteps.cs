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

    [Then(@"a payment is taken for (.*) then the result should be successful")]
    public async Task ThenAPaymentIsTakenForThenTheResultShouldBeSuccessful(double p0)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        
        var result = await _driver.TakePaymentFor("James", p0);
        
        result.Should().BeTrue();
    }

    [Then(@"a payment is taken for (.*) then the result should be unsuccessful")]
    public async Task ThenAPaymentIsTakenForThenTheResultShouldBeUnSuccessful(double p0)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        
        var result = await _driver.TakePaymentWithoutAuth("James", p0);
        
        result.Should().BeFalse();
    }
}