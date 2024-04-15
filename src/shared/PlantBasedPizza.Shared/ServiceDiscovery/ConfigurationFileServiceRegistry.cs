namespace PlantBasedPizza.Shared.ServiceDiscovery;

public class ConfigurationFileServiceRegistry : IServiceRegistry
{
    public Task<string> GetServiceAddress(string serviceName)
    {
        return Task.FromResult("");
    }
}