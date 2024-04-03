using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PlantBasedPizza.Shared.ServiceDiscovery;

public class ConsulRegisterService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly ServiceDiscoverySettings _discoverySettings;
    private readonly ILogger<ConsulRegisterService> _logger;

    public ConsulRegisterService(IConsulClient consulClient, IOptions<ServiceDiscoverySettings> discoverySettings, ILogger<ConsulRegisterService> logger)
    {
        _consulClient = consulClient;
        _logger = logger;
        _discoverySettings = discoverySettings.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var myUri = new Uri(_discoverySettings.MyUrl);

        var serviceRegistration = new AgentServiceRegistration()
        {
            Address = myUri.Host,
            Name = _discoverySettings.ServiceName,
            Port = myUri.Port,
            ID = _discoverySettings.ServiceId,
            Tags = new[] { _discoverySettings.ServiceName }
        };
        
        await _consulClient.Agent.ServiceDeregister(_discoverySettings.ServiceId, cancellationToken);
        await _consulClient.Agent.ServiceRegister(serviceRegistration, cancellationToken);
        
        this._logger.LogInformation("Service Registered with Consul");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _consulClient.Agent.ServiceDeregister(_discoverySettings.ServiceId, cancellationToken);
            
            this._logger.LogInformation("Service de-registered from Consul");
        }
        catch(Exception ex)
        {
            this._logger.LogError("Failure de-registering service", ex);
        }
    }
}