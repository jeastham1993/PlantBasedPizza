using System.Globalization;
using Datadog.Trace;
using Microsoft.AspNetCore.Http;

namespace PlantBasedPizza.Shared.Logging;

public class TraceMiddleware
{
    private readonly RequestDelegate _next;

    public TraceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var methodPath = $"{context.Request.Method} {context.Request.PathBase.Value}";

        using var requestTrace = Tracer.Instance.StartActive(methodPath);
        
        
        await _next(context);
    }
}