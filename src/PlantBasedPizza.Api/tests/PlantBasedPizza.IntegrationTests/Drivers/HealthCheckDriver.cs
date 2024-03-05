using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PlantBasedPizza.IntegrationTests.ViewModels;

namespace PlantBasedPizza.IntegrationTests.Drivers
{
    public class HealthCheckDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;

        public HealthCheckDriver()
        {
            this._httpClient = new HttpClient();
        }

        public async Task<bool> HealthCheck()
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/health"))
                .ConfigureAwait(false);

            return result.IsSuccessStatusCode;
        }
    }
}