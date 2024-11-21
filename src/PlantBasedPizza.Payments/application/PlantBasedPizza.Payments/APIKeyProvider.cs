namespace PlantBasedPizza.Payments;

public class APIKeyProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<APIKeyProvider> _logger;

    public APIKeyProvider(IConfiguration configuration, ILogger<APIKeyProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsValidApiKey(string inboundKey)
    {
        if (string.IsNullOrEmpty(inboundKey))
        {
            return false;
        }

        var expectedApiKey = _configuration["Auth:ApiKey"];
        
        this._logger.LogInformation($"Comparing API keys. Expected: '{expectedApiKey}'. Inbound: '{inboundKey}'");
        
        return inboundKey.Equals(expectedApiKey, StringComparison.OrdinalIgnoreCase);
    }
}