namespace PlantBasedPizza.Payments;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly APIKeyProvider _apiKeyProvider;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    private const string APIKEYNAME = "APIKey";
    
    public ApiKeyAuthenticationMiddleware(RequestDelegate next, APIKeyProvider apiKeyProvider, ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _apiKeyProvider = apiKeyProvider;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
        {
            this._logger.LogInformation("API Key not provided");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key was not provided. ");
            return;
        }

        var providedApiKey = extractedApiKey[0];

        if (!_apiKeyProvider.IsValidApiKey(providedApiKey))
        {
            this._logger.LogInformation("Unauthorized");
            
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Unauthorized client.");
            return;
        }

        await _next(context);
    }
}