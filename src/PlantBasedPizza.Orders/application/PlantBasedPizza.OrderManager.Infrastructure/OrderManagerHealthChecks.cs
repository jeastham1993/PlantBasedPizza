using System.Diagnostics;
using Consul;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Shared.ServiceDiscovery;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderManagerHealthChecks
{
    private readonly HttpClient _httpClient;
    private readonly ServiceEndpoints _serviceEndpoints;

    public OrderManagerHealthChecks(HttpClient client, IServiceRegistry serviceRegistry, IOptions<ServiceEndpoints> serviceEndpoints)
    {
        this._serviceEndpoints = serviceEndpoints.Value;
        
        this._httpClient = client;
    }
    
    public async Task<OrderManagerHealthCheckResult> Check()
    {
        var result = new OrderManagerHealthCheckResult();
        
        try
        {
            var res = await _httpClient.GetAsync($"{_serviceEndpoints.Recipes}/health");

            if (!res.IsSuccessStatusCode)
            {
                result.RecipeHttpStatus = "Offline";
            }
            
            Activity.Current?.AddTag("recipe.api", res.IsSuccessStatusCode);
        }
        catch (Exception)
        {
            Activity.Current?.AddTag("recipe.healthy", false);
        
            result.RecipeHttpStatus = "Offline";
        }

        return result;
    }
}

public record OrderManagerHealthCheckResult
{
    public string RecipeHttpStatus { get; set; } = "OK";
}