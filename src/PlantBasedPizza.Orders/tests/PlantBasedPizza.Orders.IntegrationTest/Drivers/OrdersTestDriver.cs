using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dapr.Client;
using PlantBasedPizza.IntegrationTest.Helpers;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.Orders.IntegrationTest.ViewModels;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.IntegrationTest.Drivers;

public class OrdersTestDriver
{
    private readonly DaprClient _daprClient;
    private readonly HttpClient _userHttpClient;
    private readonly HttpClient _staffHttpClient;

    public OrdersTestDriver()
    {
        var userToken = TestTokenGenerator.GenerateTestTokenForRole("user");
        var staffToken = TestTokenGenerator.GenerateTestTokenForRole("staff");

        _userHttpClient = new HttpClient();
        _userHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        _staffHttpClient = new HttpClient();
        _staffHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", staffToken);

        _daprClient = new DaprClientBuilder()
            .UseGrpcEndpoint("http://localhost:5101")
            .Build();
    }

    public async Task SimulateLoyaltyPointsUpdatedEvent(string customerIdentifier, decimal totalPoints,
        string? eventId = null)
    {
        await _daprClient.PublishEventAsync("public", "loyalty.customerLoyaltyPointsUpdated.v1",
            new CustomerLoyaltyPointsUpdatedEvent
            {
                CustomerIdentifier = customerIdentifier,
                TotalLoyaltyPoints = totalPoints
            }, new Dictionary<string, string>(1)
            {
                { "Cloudevents.id", eventId ?? Guid.NewGuid().ToString() }
            });

        // Delay to allow for message processing
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    public async Task SimulatePaymentSuccessEvent(string orderIdentifier, decimal paymentValue, string? eventId = null)
    {
        await _daprClient.PublishEventAsync("public", "payments.paymentSuccessful.v1", new PaymentSuccessfulEventV1
        {
            OrderIdentifier = orderIdentifier,
            Amount = paymentValue
        }, new Dictionary<string, string>(1)
        {
            { "Cloudevents.id", eventId ?? Guid.NewGuid().ToString() }
        });

        // Delay to allow for message processing
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    public async Task<Order> AddNewDeliveryOrder(string customerIdentifier)
    {
        var response = await _userHttpClient.PostAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/deliver"),
            new StringContent(
                JsonSerializer.Serialize(new CreateDeliveryOrder
                {
                    CustomerIdentifier = customerIdentifier,
                    AddressLine1 = "My test address",
                    AddressLine2 = string.Empty,
                    AddressLine3 = string.Empty,
                    AddressLine4 = string.Empty,
                    AddressLine5 = string.Empty,
                    Postcode = "TYi9PO"
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode) throw new Exception($"Request failed: {response.StatusCode}");

        return JsonSerializer.Deserialize<Order>(await response.Content.ReadAsStringAsync());
    }

    public async Task<Order> AddNewOrder(string customerIdentifier)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));

        var response = await _userHttpClient.PostAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/pickup"),
            new StringContent(
                JsonSerializer.Serialize(new CreatePickupOrderCommand
                {
                    CustomerIdentifier = customerIdentifier
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode) throw new Exception($"Request failed: {response.StatusCode}");

        return JsonSerializer.Deserialize<Order>(await response.Content.ReadAsStringAsync());
    }

    public async Task AddItemToOrder(string orderIdentifier, string recipeIdentifier, int quantity)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));

        var response = await _userHttpClient.PostAsync(
            new Uri($"{TestConstants.DefaultTestUrl}/order/{orderIdentifier}/items"),
            new StringContent(
                JsonSerializer.Serialize(new AddItemToOrderCommand
                {
                    OrderIdentifier = orderIdentifier,
                    RecipeIdentifier = recipeIdentifier,
                    Quantity = quantity
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode) throw new Exception($"Request failed: {response.StatusCode}");
    }

    public async Task SubmitOrder(string orderIdentifier)
    {
        var response = await _userHttpClient.PostAsync(
            new Uri($"{TestConstants.DefaultTestUrl}/order/{orderIdentifier}/submit"),
            new StringContent(string.Empty, Encoding.UTF8, "application/json")).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode) throw new Exception($"Request failed: {response.StatusCode}");
    }

    public async Task CollectOrder(string orderIdentifier)
    {
        // Delay to allow async processing to catch up
        await Task.Delay(TimeSpan.FromSeconds(2));

        var res = await _staffHttpClient.PostAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/collected"),
            new StringContent(
                JsonSerializer.Serialize(new CollectOrderRequest
                {
                    OrderIdentifier = orderIdentifier
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            throw new Exception($"Collect order returned non 200 HTTP Status code: {res.StatusCode}");
    }

    public async Task<Order> GetOrder(string orderIdentifier)
    {
        var response = await _userHttpClient
            .GetAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/{orderIdentifier}/detail"))
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode) throw new Exception($"Request failed: {response.StatusCode}");

        var order = JsonSerializer.Deserialize<Order>(await response.Content.ReadAsStringAsync());

        return order;
    }
}