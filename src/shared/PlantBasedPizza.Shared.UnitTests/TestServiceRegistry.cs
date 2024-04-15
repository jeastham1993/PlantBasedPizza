using PlantBasedPizza.Shared.ServiceDiscovery;

namespace PlantBasedPizza.Shared.UnitTests;

public class TestServiceRegistry : IServiceRegistry
{
    public Task<string> GetServiceAddress(string serviceName)
    {
        return Task.FromResult("https://mytestendpoint:8080");
    }
}