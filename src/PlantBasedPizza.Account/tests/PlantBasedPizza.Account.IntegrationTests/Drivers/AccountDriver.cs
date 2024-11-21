using System.Text;
using System.Text.Json;
using PlantBasedPizza.Account.IntegrationTests.ViewModels;

namespace PlantBasedPizza.Account.IntegrationTests.Drivers
{
    public class AccountDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;

        public AccountDriver()
        {
            _httpClient = new HttpClient();
        }

        public async Task<RegisterResponse?> RegisterUser(string emailAddress, string password)
        {
            var requestBody = JsonSerializer.Serialize(new
            {
                emailAddress,
                password
            });

            var registerResult = await _httpClient.PostAsync(new Uri($"{BaseUrl}/account/register"),
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

            var loginResult = await _httpClient.PostAsync(new Uri($"{BaseUrl}/account/login"),
                new StringContent(requestBody, Encoding.Default, "application/json"));

            return loginResult.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<LoginResponse>(await loginResult.Content.ReadAsStringAsync())
                : null;
        }
    }
}