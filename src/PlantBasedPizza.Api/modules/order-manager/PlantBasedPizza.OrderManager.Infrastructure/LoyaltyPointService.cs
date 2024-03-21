using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class LoyaltyPointService : ILoyaltyPointService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoyaltyPointService> _logger;

    public LoyaltyPointService(HttpClient httpClient, IConfiguration configuration, ILogger<LoyaltyPointService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task AddLoyaltyPoints(string customerId, string orderIdentifier, decimal orderValue)
    {
        try
        {
            var createLoyaltyPointsResult = await this._httpClient.PostAsync($"{_configuration["Services:Loyalty"]}/loyalty",
                new StringContent(JsonSerializer.Serialize(new CreateLoyaltyPointRequest(customerId, orderIdentifier, orderValue)), Encoding.UTF8, new MediaTypeHeaderValue("application/json")));

            if (!createLoyaltyPointsResult.IsSuccessStatusCode)
            {
                throw new Exception("Failure sending loyalty points");
            }
        }
        catch (Exception e)
        {
            this._logger.LogInformation(e, "Failure");
            throw;
        }
    }
}

public record CreateLoyaltyPointRequest(string CustomerIdentifier, string OrderIdentifier, decimal OrderValue);