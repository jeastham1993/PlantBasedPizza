namespace PlantBasedPizza.Shared.ServiceDiscovery;

public record ServiceDiscoverySettings
{
    private string serviceId;
    
    public string MyUrl { get; set; }
    public string ConsulServiceEndpoint { get; set; }
    public string ServiceName { get; set; }

    public string ServiceId
    {
        get
        {
            if (string.IsNullOrEmpty(serviceId))
            {
                serviceId = $"{ServiceName}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }

            return serviceId;
        }
    }
}