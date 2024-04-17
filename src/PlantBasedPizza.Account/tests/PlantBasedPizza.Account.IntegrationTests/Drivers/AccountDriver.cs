using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Account.IntegrationTests.ViewModels;

namespace PlantBasedPizza.Account.IntegrationTests.Drivers
{
    public class AccountDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;

        public AccountDriver()
        {
            this._httpClient = new HttpClient();
        }

        public async Task<RegisterResponse?> RegisterUser(string emailAddress, string password)
        {
            var requestBody = JsonSerializer.Serialize(new
            {
                emailAddress,
                password
            });

            var registerResult = await this._httpClient.PostAsync(new Uri($"{BaseUrl}/account/register"),
                new StringContent(requestBody, Encoding.Default, "application/json"));
            
            return registerResult.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<RegisterResponse>(await registerResult.Content.ReadAsStringAsync())
                : null;
        }

        public async Task<LoginResponse?> Login(string emailAddress, string password)
        {
            var requestBody = JsonSerializer.Serialize(new
            {
                emailAddress,
                password
            });

            var loginResult = await this._httpClient.PostAsync(new Uri($"{BaseUrl}/account/login"),
                new StringContent(requestBody, Encoding.Default, "application/json"));

            return loginResult.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<LoginResponse>(await loginResult.Content.ReadAsStringAsync())
                : null;
        }
    }
}