using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
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

            var kitchenRequests = JsonConvert.DeserializeObject<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequest>> GetPreparing()
        {
            var result = await _httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/prep")).ConfigureAwait(false);

            var kitchenRequests = JsonConvert.DeserializeObject<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequest>> GetBaking()
        {
            var result = await _httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/baking")).ConfigureAwait(false);

            var kitchenRequests = JsonConvert.DeserializeObject<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequest>> GetQualityChecked()
        {
            var result = await _httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/quality-check")).ConfigureAwait(false);

            var kitchenRequests = JsonConvert.DeserializeObject<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }

        public async Task Preparing(string orderIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            await _httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/preparing"), new StringContent(string.Empty, Encoding.UTF8)).ConfigureAwait(false);
        }
        
        public async Task PrepComplete(string orderIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            await _httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/prep-complete"), new StringContent(string.Empty, Encoding.UTF8)).ConfigureAwait(false);
        }
        
        public async Task BakeComplete(string orderIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            await _httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/bake-complete"), new StringContent(string.Empty, Encoding.UTF8)).ConfigureAwait(false);
        }
        
        public async Task QualityChecked(string orderIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            await _httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/quality-check"), new StringContent(string.Empty, Encoding.UTF8)).ConfigureAwait(false);
        }
    }
}