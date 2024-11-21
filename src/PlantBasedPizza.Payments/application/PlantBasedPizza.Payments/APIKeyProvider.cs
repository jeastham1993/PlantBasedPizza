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
        
        _logger.LogInformation($"Comparing expected API Key: {expectedApiKey} to provided API key {inboundKey}");
        
        return inboundKey.Equals(expectedApiKey, StringComparison.OrdinalIgnoreCase);
    }
}