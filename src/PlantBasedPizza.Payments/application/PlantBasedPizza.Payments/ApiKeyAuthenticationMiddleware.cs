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
        _logger.LogInformation("Received request to {Path}", context.Request.Path);
        
        if (context.Request.Path.StartsWithSegments("/health") || (context.Request.Path.Value ?? "").Contains("dapr"))
        {
            _logger.LogInformation("Internal call, skipping auth");
            await _next(context);
            return;
        }
        
        if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
        {
            _logger.LogInformation("API Key not provided");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key was not provided. ");
            return;
        }

        var providedApiKey = extractedApiKey[0];

        if (!_apiKeyProvider.IsValidApiKey(providedApiKey))
        {
            _logger.LogInformation($"Unauthorized");
            
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Unauthorized client.");
            return;
        }

        await _next(context);
    }
}