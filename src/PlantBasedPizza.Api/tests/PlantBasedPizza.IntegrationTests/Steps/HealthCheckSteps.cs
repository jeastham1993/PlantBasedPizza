using System;
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
    private readonly WireMockServer server;

    public HealthCheckSteps(ScenarioContext scenarioContext)
    {
        this._driver = new HealthCheckDriver();
        server = WireMockServer.Start(7568);
        server.Given(
                Request.Create().WithPath("/loyalty/health").UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
            );
    }
    
    [Given(@"the application is running")]
    public void GivenTheApplicationIsRunning()
    {
        // Given required to startup the application.
    }

    [Then(@"a (.*) status code is returned")]
    public async Task ThenAStatusCodeIsReturned(int p0)
    {
        var res = await this._driver.HealthCheck();

        res.Should().BeTrue();
    }
}