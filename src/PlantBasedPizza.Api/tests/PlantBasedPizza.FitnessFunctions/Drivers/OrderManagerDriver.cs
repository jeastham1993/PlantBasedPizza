using System.Text;
using System.Text.Json;
using PlantBasedPizza.FitnessFunctions.ViewModels;

namespace PlantBasedPizza.FitnessFunctions.Drivers
{
    public class OrderManagerDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public OrderManagerDriver()
        {
            this._httpClient = new HttpClient();
        }

        public async Task AddNewDeliveryOrder(string orderIdentifier)
        {
            await this._httpClient.PostAsync(new Uri($"{BaseUrl}/order/deliver"), new StringContent(
                JsonSerializer.Serialize(new CreateDeliveryOrder()
                {
                    OrderIdentifier = orderIdentifier,
                    CustomerIdentifier = "James",
                    AddressLine1 = "My test address",
                    AddressLine2 = string.Empty,
                    AddressLine3 = string.Empty,
                    AddressLine4 = string.Empty,
                    AddressLine5 = string.Empty,
                    Postcode = "TYi9PO"
                }, _jsonSerializerOptions), Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task AddNewOrder(string orderIdentifier)
        {
            await this._httpClient.PostAsync(new Uri($"{BaseUrl}/order/pickup"), new StringContent(
                JsonSerializer.Serialize(new CreatePickupOrderCommand()
                {
                    OrderIdentifier = orderIdentifier,
                    CustomerIdentifier = "James"
                },_jsonSerializerOptions), Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task AddItemToOrder(string orderIdentifier, string recipeIdentifier, int quantity)
        {
            await checkRecipeExists(recipeIdentifier).ConfigureAwait(false);

            await this._httpClient.PostAsync(new Uri($"{BaseUrl}/order/{orderIdentifier}/items"),
                new StringContent(
                    JsonSerializer.Serialize(new AddItemToOrderCommand()
                    {
                        OrderIdentifier = orderIdentifier,
                        RecipeIdentifier = recipeIdentifier,
                        Quantity = quantity
                    },_jsonSerializerOptions), Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task SubmitOrder(string orderIdentifier)
        {
            var body = JsonSerializer.Serialize(new
            {
                OrderIdentifier = orderIdentifier,
                CustomerIdentifier = "James"
            },_jsonSerializerOptions);
            await this._httpClient.PostAsync(new Uri($"{BaseUrl}/order/{orderIdentifier}/submit"),
                new StringContent(body, Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task CollectOrder(string orderIdentifier)
        {
            var res = await this._httpClient.PostAsync(new Uri($"{BaseUrl}/order/collected"), new StringContent(
                JsonSerializer.Serialize(new CollectOrderRequest()
                {
                    OrderIdentifier = orderIdentifier
                },_jsonSerializerOptions), Encoding.UTF8, "application/json")).ConfigureAwait(false);

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"Collect order returned non 200 HTTP Status code: {res.StatusCode}");
            }
        }

        public async Task<Order> GetOrder(string orderIdentifier)
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/order/{orderIdentifier}/detail"))
                .ConfigureAwait(false);

            var order = JsonSerializer.Deserialize<Order>(await result.Content.ReadAsStringAsync(),_jsonSerializerOptions);

            return order;
        }

        private async Task checkRecipeExists(string recipeIdentifier)
        {
            await this._httpClient.PostAsync($"{BaseUrl}/recipes", new StringContent(
                JsonSerializer.Serialize(new CreateRecipeCommand()
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
                },_jsonSerializerOptions), Encoding.UTF8, "application/json"));
        }
    }
}