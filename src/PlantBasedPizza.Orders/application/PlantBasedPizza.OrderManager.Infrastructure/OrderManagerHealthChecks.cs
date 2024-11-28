using System.Diagnostics;
using Dapr.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderManagerHealthChecks
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<ServiceEndpoints> _serviceEndpoints;
    private readonly Loyalty.LoyaltyClient _loyaltyClient;
    private readonly ILogger<OrderManagerHealthChecks> _logger;

    public OrderManagerHealthChecks(IOptions<ServiceEndpoints> serviceEndpoints, DaprClient daprClient, ILogger<OrderManagerHealthChecks> logger, Loyalty.LoyaltyClient loyaltyClient)
    {
        _httpClient = DaprClient.CreateInvokeHttpClient();
        _serviceEndpoints = serviceEndpoints;
        _loyaltyClient = loyaltyClient;
        _logger = logger;
    }

    public async Task<OrderManagerHealthCheckResult> Check()
    {
        var result = new OrderManagerHealthCheckResult();

        try
        {
            var res = await _httpClient.GetAsync($"http://{_serviceEndpoints.Value.Recipes}/recipes/health");

            if (!res.IsSuccessStatusCode) result.RecipeHttpStatus = "Offline";

            Activity.Current?.AddTag("recipe.api", res.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Healthcheck failure for recipe service");
            
            Activity.Current?.AddTag("recipe.healthy", false);

            result.RecipeHttpStatus = "Offline";
        }

        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));
            var metadata = new Metadata()
            {
                { "dapr-app-id", "loyaltyinternal" }
            };
            var response = await _loyaltyClient.GetCustomerLoyaltyPointsAsync(new GetCustomerLoyaltyPointsRequest()
                { CustomerIdentifier = "james" }, metadata);
            
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Healthcheck failure for internal loyalty service");
            
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