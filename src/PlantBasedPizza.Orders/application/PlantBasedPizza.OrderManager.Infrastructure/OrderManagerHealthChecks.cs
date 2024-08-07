using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderManagerHealthChecks
{
    private readonly HttpClient _httpClient;
    private readonly ServiceEndpoints _serviceEndpoints;

    public OrderManagerHealthChecks(HttpClient client, IOptions<ServiceEndpoints> serviceEndpoints)
    {
        this._serviceEndpoints = serviceEndpoints.Value;
        
        this._httpClient = client;
    }
    
    public async Task<OrderManagerHealthCheckResult> Check()
    {
        var result = new OrderManagerHealthCheckResult();
        
        try
        {
            var res = await _httpClient.GetAsync($"{_serviceEndpoints.Recipes}/recipes/health");

            if (!res.IsSuccessStatusCode)
            {
                result.RecipeHttpStatus = "Offline";
            }
        }
        catch (Exception)
        {
            result.RecipeHttpStatus = "Offline";
        }

        return result;
    }
}

public record OrderManagerHealthCheckResult
{
    public string RecipeHttpStatus { get; set; } = "OK";
}