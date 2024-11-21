using System.Diagnostics;
using FluentAssertions;
using PlantBasedPizza.E2ETests.Drivers;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.E2ETests.Steps;

[Binding]
public class HealthCheckSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly HealthCheckDriver _driver;
    private bool loyaltyPointOnline = true;

    public HealthCheckSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _driver = new HealthCheckDriver();
    }
    
    [Given(@"the application is running")]
    public void GivenTheApplicationIsRunning()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        // Given required to startup the application.
    }
    
    [When(@"the loyalty point service is offline")]
    public void WhenTheLoyaltyPointServiceIsOffline()
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        loyaltyPointOnline = false;
    }

    [Then(@"a (.*) status code is returned")]
    public async Task ThenAStatusCodeIsReturned(int p0)
    {
        Activity.Current = _scenarioContext.Get<Activity>("Activity");
        var res = await _driver.HealthCheck(loyaltyPointOnline);

        res.Should().Be(p0);
    }
}