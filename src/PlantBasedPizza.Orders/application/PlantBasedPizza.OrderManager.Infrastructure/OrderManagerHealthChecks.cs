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
    private readonly GrpcChannel _grpcChannel;

    public OrderManagerHealthChecks(HttpClient client, IServiceRegistry serviceRegistry, IOptions<ServiceEndpoints> serviceEndpoints)
    {
        this._serviceEndpoints = serviceEndpoints.Value;
        
        this._httpClient = client;

        var address = serviceRegistry.GetServiceAddress("PlantBasedPizza-LoyaltyPoints-Internal").GetAwaiter().GetResult();

        if (string.IsNullOrEmpty(address))
        {
            address = _serviceEndpoints.LoyaltyInternal;
        }
        
        this._grpcChannel = GrpcChannel.ForAddress(address);
    }
    
    public async Task<OrderManagerHealthCheckResult> Check()
    {
        var result = new OrderManagerHealthCheckResult();
        
        try
        {
            var res = await _httpClient.GetAsync($"{_serviceEndpoints.Loyalty}/loyalty/health");

            if (!res.IsSuccessStatusCode)
            {
                result.LoyaltyHttpStatus = "Offline";
            }
            
            Activity.Current?.AddTag("loyalty.healthy", res.IsSuccessStatusCode);
        }
        catch (Exception)
        {
            Activity.Current?.AddTag("loyalty.healthy", false);
        
            result.LoyaltyHttpStatus = "Offline";
        }
        
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

        try
        {
            using var source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(3));
            
            await this._grpcChannel.ConnectAsync(source.Token);
        }
        catch (Exception)
        {
            Activity.Current?.AddTag("loyalty.gRPC.healthy", false);
            result.LoyaltyGrpcStatus = "Offline";
        }

        return result;
    }
}

public record OrderManagerHealthCheckResult
{
    public string LoyaltyHttpStatus { get; set; } = "OK";
    public string LoyaltyGrpcStatus { get; set; } = "OK";
    public string RecipeHttpStatus { get; set; } = "OK";
}