namespace PlantBasedPizza.Payments;

public class APIKeyProvider
{
    private readonly IConfiguration _configuration;

    public APIKeyProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsValidApiKey(string inboundKey)
    {
        return inboundKey.Equals(_configuration["Auth:ApiKey"]);
    }
}