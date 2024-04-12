using System.Diagnostics;
using Consul;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using PlantBasedPizza.Shared.ServiceDiscovery;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderManagerHealthChecks
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly GrpcChannel _grpcChannel;

    public OrderManagerHealthChecks(HttpClient client, IServiceRegistry serviceRegistry, IConfiguration configuration)
    {
        this._httpClient = client;
        _configuration = configuration;

        var address = serviceRegistry.GetServiceAddress("PlantBasedPizza-LoyaltyPoints-Internal").GetAwaiter().GetResult();
        this._grpcChannel = GrpcChannel.ForAddress(address ?? configuration["Services:LoyaltyInternal"]);
    }
    
    public async Task<OrderManagerHealthCheckResult> Check()
    {
        var result = new OrderManagerHealthCheckResult();
        
        try
        {
            var res = await _httpClient.GetAsync($"{this._configuration["Services:Loyalty"]}/loyalty/health");

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
            var res = await _httpClient.GetAsync($"{this._configuration["Services:Recipes"]}/health");

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