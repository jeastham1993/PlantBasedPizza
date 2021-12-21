using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlantBasedPizza.IntegrationTests.Requests;
using PlantBasedPizza.IntegrationTests.ViewModels;

namespace PlantBasedPizza.IntegrationTests.Drivers
{
    public class DeliveryDriver
    {
        private static string BaseUrl;

        private readonly HttpClient _httpClient;

        public DeliveryDriver()
        {
            BaseUrl = Environment.GetEnvironmentVariable("TEST_URL") ?? @"http://plant-publi-1ce809ri0ilmj-684717832.eu-west-1.elb.amazonaws.com";

            this._httpClient = new HttpClient();
        }

        public async Task<List<DeliveryRequest>> GetAwaitingDriver()
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/delivery/delivery/awaiting-collection"))
                .ConfigureAwait(false);

            var deliveryRequests =
                JsonConvert.DeserializeObject<List<DeliveryRequest>>(await result.Content.ReadAsStringAsync());

            return deliveryRequests;
        }

        public async Task AssignDriver(string orderIdentifier, string driverName)
        {
            var url = $"{BaseUrl}/delivery/delivery/assign";
            
            Console.WriteLine(url);

            var content = JsonConvert.SerializeObject(new AssignDriverRequest()
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
            var url = $"{BaseUrl}/delivery/delivery/delivered";
            
            Console.WriteLine(url);

            var content = JsonConvert.SerializeObject(new MarkOrderDeliveredRequest()
            {
                OrderIdentifier = orderIdentifier
            });

            var result = await this._httpClient.PostAsync(new Uri(url), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task<List<DeliveryRequest>> GetAssignedDeliveriesForDriver(string driverName)
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/delivery/delivery/driver/{driverName}"))
                .ConfigureAwait(false);

            var deliveryRequests =
                JsonConvert.DeserializeObject<List<DeliveryRequest>>(await result.Content.ReadAsStringAsync());

            return deliveryRequests;
        }
    }
}