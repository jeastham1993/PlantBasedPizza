using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PlantBasedPizza.IntegrationTests.Hooks;

[Binding]
public class Hooks
{
    private static WireMockServer server;
    
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        server = WireMockServer.Start(7568);
        server.Given(
                Request.Create()
                    .WithPath("/loyalty/health")
                    .UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
            );
        server.Given(
                Request.Create()
                    .WithPath("/loyalty")
                    .UsingPost()
                )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                );
    }
}