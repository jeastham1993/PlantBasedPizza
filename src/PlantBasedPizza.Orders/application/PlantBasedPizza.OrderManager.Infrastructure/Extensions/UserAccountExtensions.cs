using System.Diagnostics;
using System.Security.Claims;
using Datadog.Trace;

namespace PlantBasedPizza.OrderManager.Infrastructure.Extensions;

public static class UserAccountExtensions
{
    public static string ExtractAccountId(this IEnumerable<Claim> claims)
    {
        var accountId = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
        
        Tracer.Instance.ActiveScope?.Span.SetTag("accountId", accountId);
        
        return accountId;
    }
}