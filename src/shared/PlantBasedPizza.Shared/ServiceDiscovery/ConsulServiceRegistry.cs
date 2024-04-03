using System.Security.Cryptography;
using Consul;
using Microsoft.Extensions.Logging;

namespace PlantBasedPizza.Shared.ServiceDiscovery;

public class ConsulServiceRegistry : IServiceRegistry
{
    private readonly IConsulClient _consulClient;
    private readonly ILogger<ConsulServiceRegistry> _logger;

    public ConsulServiceRegistry(IConsulClient consulClient, ILogger<ConsulServiceRegistry> logger)
    {
        _consulClient = consulClient;
        _logger = logger;
    }

    public async Task<string> GetServiceAddress(string serviceName)
    {
        var services = await _consulClient.Health.Service(serviceName);
        
        this._logger.LogInformation($"Found {services.Response.Length} service(s) for {serviceName}");

        if (services.Response.Length == 1)
        {
            var singleService = services.Response[0];
            
            this._logger.LogInformation($"Returning address: {singleService.Service.Address}:{singleService.Service.Port}");
            
            return $"http://{singleService.Service.Address}:{singleService.Service.Port}";
        }

        var indexToUse = RandomNumberGenerator.GetInt32(0, services.Response.Length - 1);

        var service = services.Response[indexToUse];
        
        this._logger.LogInformation($"Returning address: {service.Service.Address}");
        
        return $"http://{service.Service.Address}:{service.Service.Port}";
    }
}