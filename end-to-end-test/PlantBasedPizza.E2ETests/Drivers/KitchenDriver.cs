using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PlantBasedPizza.E2ETests.ViewModels;
using PlantBasedPizza.IntegrationTest.Helpers;

namespace PlantBasedPizza.E2ETests.Drivers
{
    public class KitchenDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;

        public KitchenDriver()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateTestTokenForRole("staff"));
        }
        
        public async Task<List<KitchenRequest>> GetNew()
        {
            // Delay to allow async processing to catch up
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            var result = await _httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/new")).ConfigureAwait(false);

            var kitchenRequests = JsonSerializer.Deserialize<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequest>> GetPreparing()
        {
            var result = await _httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/prep")).ConfigureAwait(false);

            var kitchenRequests = JsonSerializer.Deserialize<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequest>> GetBaking()
        {
            var result = await _httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/baking")).ConfigureAwait(false);

            var kitchenRequests = JsonSerializer.Deserialize<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequest>> GetQualityChecked()
        {
            var result = await _httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/quality-check")).ConfigureAwait(false);

            var kitchenRequests = JsonSerializer.Deserialize<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }

        public async Task Preparing(string orderIdentifier)
        {
            // Longer delay here to allow for order outbox to pick up and publish the event
            await Task.Delay(TimeSpan.FromSeconds(20));
            
            var result = await _httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/preparing"), new StringContent(JsonSerializer.Serialize(new
            {
                orderIdentifier
            }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Unexected response from kitchen API: {result.StatusCode}");
            }
        }
        
        public async Task PrepComplete(string orderIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var result = await _httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/prep-complete"), new StringContent(JsonSerializer.Serialize(new
            {
                orderIdentifier
            }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Unexected response from kitchen API: {result.StatusCode}");
            }
        }
        
        public async Task BakeComplete(string orderIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var result = await _httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/bake-complete"), new StringContent(JsonSerializer.Serialize(new
            {
                orderIdentifier
            }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Unexected response from kitchen API: {result.StatusCode}");
            }
        }
        
        public async Task QualityChecked(string orderIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var result = await _httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/quality-check"), new StringContent(JsonSerializer.Serialize(new
            {
                orderIdentifier
            }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Unexected response from kitchen API: {result.StatusCode}");
            }
        }
    }
}