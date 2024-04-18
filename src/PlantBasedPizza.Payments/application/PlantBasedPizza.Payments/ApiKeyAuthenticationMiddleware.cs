namespace PlantBasedPizza.Payments;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly APIKeyProvider _apiKeyProvider;
    private const string APIKEYNAME = "APIKey";
    
    public ApiKeyAuthenticationMiddleware(RequestDelegate next, APIKeyProvider apiKeyProvider)
    {
        _next = next;
        _apiKeyProvider = apiKeyProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key was not provided. ");
            return;
        }

        var providedApiKey = extractedApiKey[0];

        if (!_apiKeyProvider.IsValidApiKey(providedApiKey))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Unauthorized client. ");
            return;
        }

        await _next(context);
    }
}