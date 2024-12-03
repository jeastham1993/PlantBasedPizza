using System.Diagnostics;
using System.Security.Claims;

namespace PlantBasedPizza.Shared.Logging;

public static class UserAccountExtensions
{
    public static string? ExtractAccountId(this IEnumerable<Claim> claims)
    {
        var claimsList = claims.ToList();
        
        claimsList.AddUserDetailsToTelemetry();
        
        var accountId = claimsList.Find(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return accountId;
    }
    
    internal static void AddUserDetailsToTelemetry(this List<Claim> claims)
    {
        var accountId = claims.Find(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        var userType = claims.Find(c => c.Type == "UserType")?.Value;
        var userTier = claims.Find(c => c.Type == "UserTier")?.Value;
        var userAccountAge = claims.Find(c => c.Type == "AccountAge")?.Value;
        
        Activity.Current?.SetTag("user.id", accountId ?? "");
        Activity.Current?.AddTag("user.type", userType ?? "");
        Activity.Current?.AddTag("user.tier", userTier ?? "");
        Activity.Current?.AddTag("user.account_age", userAccountAge ?? "");
    }
}