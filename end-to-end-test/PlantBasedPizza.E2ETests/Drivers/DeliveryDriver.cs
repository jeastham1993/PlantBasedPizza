using System.Text;
using System.Text.Json;
using PlantBasedPizza.E2ETests.ViewModels;

namespace PlantBasedPizza.E2ETests.Drivers
{
    public class DeliveryDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;

        public DeliveryDriver()
        {
            this._httpClient = new HttpClient();
        }

        public async Task<List<DeliveryRequest>> GetAwaitingDriver()
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/delivery/awaiting-collection"))
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

            var result = await this._httpClient.PostAsync(new Uri(url), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);

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

            await this._httpClient.PostAsync(new Uri(url), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task<List<DeliveryRequest>> GetAssignedDeliveriesForDriver(string driverName)
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/delivery/driver/{driverName}/orders"))
                .ConfigureAwait(false);

            var deliveryRequests =
                JsonSerializer.Deserialize<List<DeliveryRequest>>(await result.Content.ReadAsStringAsync());

            return deliveryRequests;
        }
    }
}