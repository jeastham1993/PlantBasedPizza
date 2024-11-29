using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dapr.Client;
using PlantBasedPizza.Delivery.IntegrationTests.ViewModels;
using PlantBasedPizza.IntegrationTest.Helpers;

namespace PlantBasedPizza.Delivery.IntegrationTests.Drivers
{
    public class DeliveryDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _userHttpClient;
        private readonly HttpClient _staffHttpClient;
        private readonly HttpClient _driverHttpClient;
        private readonly DaprClient _daprClient;

        public DeliveryDriver()
        {
            _userHttpClient = new HttpClient();
            _userHttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateTestTokenForRole("user"));
            
            _staffHttpClient = new HttpClient();
            _staffHttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateTestTokenForRole("staff"));
            
            _driverHttpClient = new HttpClient();
            _driverHttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateTestTokenForRole("driver"));
            _daprClient = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:5101")
                .Build();
        }

        public async Task ANewOrderIsReadyForDelivery(string orderIdentifier, string? eventId = null)
        {
            await _daprClient.PublishEventAsync("public", "order.readyForDelivery.v1",
                new OrderReadyForDeliveryEventV1()
                {
                    OrderIdentifier = orderIdentifier,
                    DeliveryAddressLine1 = "Address Line 1",
                    DeliveryAddressLine2 = "Address Line 2",
                    DeliveryAddressLine3 = "Address Line 3",
                    DeliveryAddressLine4 = "Address Line 4",
                    DeliveryAddressLine5 = "Address Line 5",
                    Postcode = "TL6 7IO",
                }, new Dictionary<string, string>(2)
                {
                    { "cloudevent.source", "kitchen" },
                    { "cloudevent.type", "kitchen.driverCollectedOrder.v1" },
                    { "cloudevent.id", eventId ?? Guid.NewGuid().ToString() }
                });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        public async Task<List<DeliveryRequest>> GetAwaitingDriver()
        {
            var result = await _staffHttpClient.GetAsync(new Uri($"{BaseUrl}/delivery/awaiting-collection"))
                .ConfigureAwait(false);

            var deliveryRequests =
                JsonSerializer.Deserialize<List<DeliveryRequest>>(await result.Content.ReadAsStringAsync());

            return deliveryRequests;
        }

        public async Task AssignDriver(string orderIdentifier, string driverName)
        {
            var url = $"{BaseUrl}/delivery/assign";
            
            Console.WriteLine(url);

            var content = JsonSerializer.Serialize(new AssignDriverRequest()
            {
                OrderIdentifier = orderIdentifier,
                DriverName = driverName
            });

            var result = await _staffHttpClient.PostAsync(new Uri(url), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to assign driver: {result.StatusCode} {await result.Content.ReadAsStringAsync()}");
            }
        }

        public async Task DeliverOrder(string orderIdentifier)
        {
            var url = $"{BaseUrl}/delivery/delivered";
            
            Console.WriteLine(url);

            var content = JsonSerializer.Serialize(new MarkOrderDeliveredRequest()
            {
                OrderIdentifier = orderIdentifier
            });

            await _driverHttpClient.PostAsync(new Uri(url), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task<List<DeliveryRequest>> GetAssignedDeliveriesForDriver(string driverName)
        {
            var result = await _staffHttpClient.GetAsync(new Uri($"{BaseUrl}/delivery/driver/{driverName}/orders"))
                .ConfigureAwait(false);

            var deliveryRequests =
                JsonSerializer.Deserialize<List<DeliveryRequest>>(await result.Content.ReadAsStringAsync());

            return deliveryRequests;
        }
    }
}