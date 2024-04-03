namespace PlantBasedPizza.Shared.ServiceDiscovery;

public interface IServiceRegistry
{
    Task<string> GetServiceAddress(string serviceName);
}