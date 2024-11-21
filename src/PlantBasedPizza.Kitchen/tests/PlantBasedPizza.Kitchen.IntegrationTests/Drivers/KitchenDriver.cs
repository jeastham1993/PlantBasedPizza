using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dapr.Client;
using PlantBasedPizza.Events;
using PlantBasedPizza.IntegrationTest.Helpers;
using PlantBasedPizza.Kitchen.IntegrationTests.ViewModels;

namespace PlantBasedPizza.Kitchen.IntegrationTests.Drivers;

public class KitchenDriver
{
    private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;
        private readonly DaprClient _daprClient;

        public KitchenDriver()
        {
            var staffToken = TestTokenGenerator.GenerateTestTokenForRole("staff");
            this._httpClient = new HttpClient();
            this._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", staffToken); 
            
            this._daprClient = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:5101")
                .Build();
        }

        public async Task NewOrderSubmitted(string orderIdentifier)
        {
            await this._daprClient.PublishEventAsync("public", "order.orderSubmitted.v1", new OrderSubmittedEventV1()
            {
                OrderIdentifier = orderIdentifier,
                Items = new List<OrderSubmittedEventItem>(1)
                {
                    new()
                    {
                        ItemName = "pizza",
                        RecipeIdentifier = "pizza"
                    }
                }
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        public async Task<List<KitchenRequestDto>> GetNewOrders()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            var result = await this._httpClient.GetAsync($"{TestConstants.DefaultTestUrl}/kitchen/new");

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Failure retrieving new kitchen orders");
            }

            return JsonSerializer.Deserialize<List<KitchenRequestDto>>(await result.Content.ReadAsStringAsync());
        }
        
        public async Task<List<KitchenRequestDto>> GetPreparing()
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/prep")).ConfigureAwait(false);

            var kitchenRequests = JsonSerializer.Deserialize<List<KitchenRequestDto>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequestDto>> GetNew()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/new")).ConfigureAwait(false);

            var kitchenRequests = JsonSerializer.Deserialize<List<KitchenRequestDto>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequestDto>> GetBaking()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/baking")).ConfigureAwait(false);

            var kitchenRequests = JsonSerializer.Deserialize<List<KitchenRequestDto>>(await result.Content.ReadAsStringAsync());

            return kitchenRequests;
        }
        
        public async Task<List<KitchenRequestDto>> GetQualityChecked()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/kitchen/quality-check")).ConfigureAwait(false);

            var kitchenRequests = JsonSerializer.Deserialize<List<KitchenRequestDto>>(await result.Content.ReadAsStringAsync());

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