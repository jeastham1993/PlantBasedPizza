using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using PlantBasedPizza.IntegrationTests.Drivers;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PlantBasedPizza.IntegrationTests.Steps;

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