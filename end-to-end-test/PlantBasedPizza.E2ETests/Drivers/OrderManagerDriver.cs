using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using PlantBasedPizza.E2ETests.Requests;
using PlantBasedPizza.E2ETests.ViewModels;
using PlantBasedPizza.IntegrationTest.Helpers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PlantBasedPizza.E2ETests.Drivers
{
    public class OrderManagerDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _userHttpClient;
        private readonly HttpClient _staffHttpClient;

        public OrderManagerDriver()
        {
            var userToken = TestTokenGenerator.GenerateTestTokenForRole("user");
            var staffToken = TestTokenGenerator.GenerateTestTokenForRole("staff");
            
            _userHttpClient = new HttpClient();
            _userHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            
            _staffHttpClient = new HttpClient();
            _staffHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", staffToken);
        }

        public async Task<Order> AddNewDeliveryOrder(string customerIdentifier)
        {
            var response = await _userHttpClient.PostAsync(new Uri($"{BaseUrl}/order/deliver"), new StringContent(
                JsonConvert.SerializeObject(new CreateDeliveryOrder()
                {
                    CustomerIdentifier = customerIdentifier,
                    AddressLine1 = "My test address",
                    AddressLine2 = string.Empty,
                    AddressLine3 = string.Empty,
                    AddressLine4 = string.Empty,
                    AddressLine5 = string.Empty,
                    Postcode = "TYi9PO"
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected response from order service: {response.StatusCode}");
            }
            
            return JsonSerializer.Deserialize<Order>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public async Task<Order> AddNewOrder(string customerIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var response = await _userHttpClient.PostAsync(new Uri($"{BaseUrl}/order/pickup"), new StringContent(
                JsonConvert.SerializeObject(new CreatePickupOrderCommand()
                {
                    CustomerIdentifier = customerIdentifier
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected response from order service: {response.StatusCode}");
            }
            
            return JsonSerializer.Deserialize<Order>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public async Task AddItemToOrder(string orderIdentifier, string recipeIdentifier, int quantity)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            await checkRecipeExists(recipeIdentifier).ConfigureAwait(false);

            await _userHttpClient.PostAsync(new Uri($"{BaseUrl}/order/{orderIdentifier}/items"),
                new StringContent(
                    JsonConvert.SerializeObject(new AddItemToOrderCommand()
                    {
                        OrderIdentifier = orderIdentifier,
                        RecipeIdentifier = recipeIdentifier,
                        Quantity = quantity
                    }), Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task SubmitOrder(string orderIdentifier)
        {
            await _userHttpClient.PostAsync(new Uri($"{BaseUrl}/order/{orderIdentifier}/submit"),
                new StringContent(string.Empty, Encoding.UTF8, "application/json")).ConfigureAwait(false);
            
            // Allow time for payment processor to run
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        public async Task CollectOrder(string orderIdentifier)
        {
            // Delay to allow async processing to catch up
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            var res = await _staffHttpClient.PostAsync(new Uri($"{BaseUrl}/order/collected"), new StringContent(
                JsonConvert.SerializeObject(new CollectOrderRequest()
                {
                    OrderIdentifier = orderIdentifier
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"Collect order returned non 200 HTTP Status code: {res.StatusCode}");
            }
        }

        public async Task<Order> GetOrder(string orderIdentifier)
        {
            var result = await _userHttpClient.GetAsync(new Uri($"{BaseUrl}/order/{orderIdentifier}/detail"))
                .ConfigureAwait(false);

            var order = JsonConvert.DeserializeObject<Order>(await result.Content.ReadAsStringAsync());

            return order;
        }

        private async Task checkRecipeExists(string recipeIdentifier)
        {
            await _staffHttpClient.PostAsync($"{BaseUrl}/recipes", new StringContent(
                JsonConvert.SerializeObject(new CreateRecipeCommand()
                {
                    RecipeIdentifier = recipeIdentifier,
                    Name = recipeIdentifier,
                    Price = 10,
                    Ingredients = new List<CreateRecipeCommandItem>(1)
                    {
                        new CreateRecipeCommandItem()
                        {
                            Name = "Pizza",
                            Quantity = 1
                        }
                    }
                }), Encoding.UTF8, "application/json"));
        }
    }
}