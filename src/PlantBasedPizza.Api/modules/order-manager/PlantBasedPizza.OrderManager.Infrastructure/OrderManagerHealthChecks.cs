using System.Diagnostics;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using PlantBasedPizza.Shared.ServiceDiscovery;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderManagerHealthChecks
{
    private readonly HttpClient _httpClient;
    private readonly GrpcChannel _grpcChannel;

    public OrderManagerHealthChecks(HttpClient client, IServiceRegistry serviceRegistry, IConfiguration configuration)
    {
        this._httpClient = client;
        
        var address = serviceRegistry.GetServiceAddress("PlantBasedPizza-LoyaltyPoints-Internal").GetAwaiter().GetResult();

        if (string.IsNullOrEmpty(address))
        {
            address = configuration["Services:LoyaltyInternal"];
        }
        
        this._grpcChannel = GrpcChannel.ForAddress(address);
    }
    
    public async Task<OrderManagerHealthCheckResult> Check()
    {
        var result = new OrderManagerHealthCheckResult();
        
        try
        {
            var res = await _httpClient.GetAsync("/loyalty/health");

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
}