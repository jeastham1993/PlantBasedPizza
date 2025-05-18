using System;
using System.Net.Http;
using System.Threading.Tasks;

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

        public async Task<int> HealthCheck(bool loyalyPointSuccess = true)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri($"{BaseUrl}/health"));
            httpRequestMessage.Headers.Add("Response", loyalyPointSuccess ? "Success" : "Failure");
            
            var result = await this._httpClient
                .SendAsync(httpRequestMessage)
                .ConfigureAwait(false);

            return (int)result.StatusCode;
        }
    }
}