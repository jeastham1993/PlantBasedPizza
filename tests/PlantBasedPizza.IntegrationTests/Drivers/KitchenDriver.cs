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
    public class KitchenDriver
    {
        private static string BaseUrl;

        private readonly HttpClient _httpClient;

        public KitchenDriver()
        {
            BaseUrl = Environment.GetEnvironmentVariable("TEST_URL") ?? @"http://plant-publi-1ce809ri0ilmj-684717832.eu-west-1.elb.amazonaws.com";
            
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