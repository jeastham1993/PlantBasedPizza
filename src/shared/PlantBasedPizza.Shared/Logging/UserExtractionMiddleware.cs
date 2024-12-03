using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace PlantBasedPizza.Shared.Logging;

public class UserExtractionMiddleware
{
    private readonly RequestDelegate _next;

    public UserExtractionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity is null)
        {
            await _next(context);
            return;
        }

        context.User.Claims.ToList().AddUserDetailsToTelemetry();
        await _next(context);
    }
}

public static class UserExtractionMiddlewareExtractions
{
    public static IApplicationBuilder UseUserExtractionMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserExtractionMiddleware>();
    }
}