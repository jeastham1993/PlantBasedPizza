using FluentAssertions;
using PlantBasedPizza.E2ETests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.E2ETests.Steps;

[Binding]
public class HealthCheckSteps
{
    private readonly HealthCheckDriver _driver;
    private bool loyaltyPointOnline = true;

    public HealthCheckSteps(ScenarioContext scenarioContext)
    {
        this._driver = new HealthCheckDriver();
    }
    
    [Given(@"the application is running")]
    public void GivenTheApplicationIsRunning()
    {
        // Given required to startup the application.
    }
    
    [When(@"the loyalty point service is offline")]
    public void WhenTheLoyaltyPointServiceIsOffline()
    {
        this.loyaltyPointOnline = false;
    }

    [Then(@"a (.*) status code is returned")]
    public async Task ThenAStatusCodeIsReturned(int p0)
    {
        var res = await this._driver.HealthCheck(loyaltyPointOnline);

        res.Should().Be(p0);
    }
}