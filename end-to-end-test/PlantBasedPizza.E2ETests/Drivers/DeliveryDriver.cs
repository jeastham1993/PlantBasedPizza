using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PlantBasedPizza.E2ETests.ViewModels;
using PlantBasedPizza.IntegrationTest.Helpers;

namespace PlantBasedPizza.E2ETests.Drivers
{
    public class DeliveryDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _staffHttpClient;
        private readonly HttpClient _driverHttpClient;

        public DeliveryDriver()
        {
            _staffHttpClient = new HttpClient();
            _staffHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateTestTokenForRole("staff"));

            _driverHttpClient = new HttpClient();
            _driverHttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateTestTokenForRole("driver"));
        }

        public async Task<List<DeliveryRequest>> GetAwaitingDriver()
        {
            // Delay to allow async processing to catch up
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            var result = await _staffHttpClient.GetAsync(new Uri($"{BaseUrl}/delivery/awaiting-collection"))
                .ConfigureAwait(false);

            var deliveryRequests =
                JsonSerializer.Deserialize<List<DeliveryRequest>>(await result.Content.ReadAsStringAsync());

            return deliveryRequests;
        }

        public async Task AssignDriver(string orderIdentifier, string driverName)
        {
            // Delay to allow async process to catch up
            await Task.Delay(TimeSpan.FromSeconds(2));
            var url = $"{BaseUrl}/delivery/assign";

            var content = JsonSerializer.Serialize(new AssignDriverRequest()
            {
                OrderIdentifier = orderIdentifier,
                DriverName = driverName
            });

            var retries = 10;
            var isSuccess = false;

            while (retries > 0)
            {
                var result = await _staffHttpClient.PostAsync(new Uri(url), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                if (result.IsSuccessStatusCode)
                {
                    isSuccess = true;
                    return;
                }

                retries--;
                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            if (!isSuccess)
            {
                throw new Exception($"Failed to assign driver");   
            }
        }

        public async Task DeliverOrder(string orderIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var url = $"{BaseUrl}/delivery/delivered";

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