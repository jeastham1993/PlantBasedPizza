using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace PlantBasedPizza.Shared.ServiceDiscovery;

public class ServiceRegistryHttpMessageHandler : DelegatingHandler
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly ServiceDiscoverySettings _serviceDiscoverySettings;
    private readonly Dictionary<string, string> _retrievedServices;
    private DateTime? _lastRetrieved;

    public ServiceRegistryHttpMessageHandler(IServiceRegistry serviceRegistry, IOptions<ServiceDiscoverySettings> serviceDiscoverySettings)
    {
        _serviceRegistry = serviceRegistry;
        _serviceDiscoverySettings = serviceDiscoverySettings.Value;
        _retrievedServices = new Dictionary<string, string>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request = await ParseRequest(request, cancellationToken);

        return await base.SendAsync(request, cancellationToken);
    }

    public async Task<HttpRequestMessage> ParseRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var serviceDiscoverySpan = Activity.Current?.Source.StartActivity("ServiceDiscovery")!;

        if (string.IsNullOrEmpty(_serviceDiscoverySettings.ConsulServiceEndpoint))
        {
            return request;
        }
        
        var current = request.RequestUri;

        if (_retrievedServices.TryGetValue(current.Host, out var service))
        {
            serviceDiscoverySpan?.AddTag("serviceDiscovery.inMemory", true);
            
            var address = new Uri($"{service}{request.RequestUri.AbsolutePath}");
            
            if (DateTime.UtcNow - _lastRetrieved.Value > TimeSpan.FromSeconds(10))
            {
                serviceDiscoverySpan?.AddTag("serviceDiscovery.retrieved", true);
                
                var updatedAddress = await retrieveAddress(current, request);

                address = new Uri($"{updatedAddress.Scheme}://{updatedAddress.Authority}{request.RequestUri.AbsolutePath}");
            }
            
            request.RequestUri = address;
            
            // Stop the span before the HTTP request pipeline continues
            serviceDiscoverySpan?.Stop();
            
            return request;
        }
        
        request.RequestUri = await retrieveAddress(current, request);
        
        serviceDiscoverySpan?.AddTag("serviceDiscovery.retrieved", true);
        
        serviceDiscoverySpan?.Stop();

        return request;
    }

    public async Task<Uri> retrieveAddress(Uri current, HttpRequestMessage request)
    {
        var discoveredHost = await _serviceRegistry.GetServiceAddress(current.Host);

        if (discoveredHost is null)
        {
            _lastRetrieved = DateTime.UtcNow;
            _retrievedServices[current.Host] = request.RequestUri.Host;
            
            return request.RequestUri;
        }
        
        _lastRetrieved = DateTime.UtcNow;
        _retrievedServices[current.Host] = discoveredHost;

        return new Uri($"{discoveredHost}{request.RequestUri.AbsolutePath}");
    }
}