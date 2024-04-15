using FluentAssertions;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Shared.ServiceDiscovery;

namespace PlantBasedPizza.Shared.UnitTests;

public class ServiceDiscoveryTests
{
    [Fact]
    public async Task ServiceDiscoveryUrlFormatter_ShouldBuildUrl()
    {
        var testOptions = Options.Create(new ServiceDiscoverySettings()
        {
            ConsulServiceEndpoint = "http://testendpoint.test"
        });

        var messageHandler = new ServiceRegistryHttpMessageHandler(new TestServiceRegistry(), testOptions);

        var newUri = await messageHandler.ParseRequest(new HttpRequestMessage(HttpMethod.Get, "https://Api/recipes/pizza"), default);

        newUri.RequestUri.ToString().Should().Be("https://mytestendpoint:8080/recipes/pizza");
    }
    
    [Fact]
    public async Task ServiceDiscoveryUrlFormatter_OnSecondRead_ShouldReadFromInMemoryCache()
    {
        var testOptions = Options.Create(new ServiceDiscoverySettings()
        {
            ConsulServiceEndpoint = "http://testendpoint.test"
        });

        var messageHandler = new ServiceRegistryHttpMessageHandler(new TestServiceRegistry(), testOptions);

        var newUri = await messageHandler.ParseRequest(new HttpRequestMessage(HttpMethod.Get, "https://Api/recipes/pizza"), default);
        await Task.Delay(TimeSpan.FromSeconds(12));
        var secondUri = await messageHandler.ParseRequest(new HttpRequestMessage(HttpMethod.Get, "https://Api/recipes/pizza"), default);

        secondUri.RequestUri.ToString().Should().Be("https://mytestendpoint:8080/recipes/pizza");
    }
}