using System.Diagnostics;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderManagerHealthChecks
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly GrpcChannel _grpcChannel;

    public OrderManagerHealthChecks(HttpClient client, IConfiguration configuration)
    {
        this._configuration = configuration;
        this._grpcChannel = GrpcChannel.ForAddress(configuration["Services:LoyaltyInternal"]);
        this._httpClient = client;
    }
    
    public async Task<OrderManagerHealthCheckResult> Check()
    {
        var result = new OrderManagerHealthCheckResult();
        
        try
        {
            var res = await _httpClient.GetAsync($"{_configuration["Services:Loyalty"]}/loyalty/health");

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