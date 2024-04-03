using FluentAssertions;
using PlantBasedPizza.Payments.IntegrationTests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Payments.IntegrationTests.Steps;

[Binding]
public sealed class PaymentSteps
{
    private readonly PaymentDriver _driver;

    public PaymentSteps()
    {
        this._driver = new PaymentDriver();
    }

    [Then(@"a payment is taken for (.*) then the result should be successful")]
    public async Task ThenAPaymentIsTakenForThenTheResultShouldBeSuccessful(double p0)
    {
        var result = await this._driver.TakePaymentFor("James", p0);
        
        result.Should().BeTrue();
    }
}