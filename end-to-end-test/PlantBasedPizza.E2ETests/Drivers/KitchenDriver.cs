using System.Text;
using Newtonsoft.Json;
using PlantBasedPizza.E2ETests.ViewModels;

namespace PlantBasedPizza.E2ETests.Drivers
{
    public class KitchenDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;

        public KitchenDriver()
        {
            this._httpClient = new HttpClient();
        }
        
        public async Task<List<KitchenRequest>> GetNew()
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/new")).ConfigureAwait(false);

            var kitchenRequests = JsonConvert.DeserializeObject<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequest>> GetPreparing()
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/prep")).ConfigureAwait(false);

            var kitchenRequests = JsonConvert.DeserializeObject<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequest>> GetBaking()
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/baking")).ConfigureAwait(false);

            var kitchenRequests = JsonConvert.DeserializeObject<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequest>> GetQualityChecked()
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/quality-check")).ConfigureAwait(false);

            var kitchenRequests = JsonConvert.DeserializeObject<List<KitchenRequest>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }

        public async Task Preparing(string orderIdentifier)
        {
            await this._httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/preparing"), new StringContent(string.Empty, Encoding.UTF8)).ConfigureAwait(false);
        }
        
        public async Task PrepComplete(string orderIdentifier)
        {
            await this._httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/prep-complete"), new StringContent(string.Empty, Encoding.UTF8)).ConfigureAwait(false);
        }
        
        public async Task BakeComplete(string orderIdentifier)
        {
            await this._httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/bake-complete"), new StringContent(string.Empty, Encoding.UTF8)).ConfigureAwait(false);
        }
        
        public async Task QualityChecked(string orderIdentifier)
        {
            await this._httpClient.PutAsync(new Uri($"{BaseUrl}/kitchen/{orderIdentifier}/quality-check"), new StringContent(string.Empty, Encoding.UTF8)).ConfigureAwait(false);
        }
    }
}