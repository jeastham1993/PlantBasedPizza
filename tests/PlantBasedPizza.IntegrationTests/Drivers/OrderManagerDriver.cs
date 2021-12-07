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
    public class OrderManagerDriver
    {
        private static string BaseUrl;

        private readonly HttpClient _httpClient;

        public OrderManagerDriver()
        {
            BaseUrl = Environment.GetEnvironmentVariable("TEST_URL") ?? @"http://localhost:5113";

            this._httpClient = new HttpClient();
        }

        public async Task AddNewDeliveryOrder(string orderIdentifier)
        {
            await this._httpClient.PostAsync(new Uri($"{BaseUrl}/order/order/deliver"), new StringContent(
                JsonConvert.SerializeObject(new CreateDeliveryOrder()
                {
                    OrderIdentifier = orderIdentifier,
                    CustomerIdentifier = "James",
                    AddressLine1 = "My test address",
                    AddressLine2 = string.Empty,
                    AddressLine3 = string.Empty,
                    AddressLine4 = string.Empty,
                    AddressLine5 = string.Empty,
                    Postcode = "TYi9PO"
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task AddNewOrder(string orderIdentifier)
        {
            await this._httpClient.PostAsync(new Uri($"{BaseUrl}/order/order/pickup"), new StringContent(
                JsonConvert.SerializeObject(new CreatePickupOrderCommand()
                {
                    OrderIdentifier = orderIdentifier,
                    CustomerIdentifier = "James"
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task AddItemToOrder(string orderIdentifier, string recipeIdentifier, int quantity)
        {
            await checkRecipeExists(recipeIdentifier).ConfigureAwait(false);

            await this._httpClient.PostAsync(new Uri($"{BaseUrl}/order/order/{orderIdentifier}/items"),
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
            await this._httpClient.PostAsync(new Uri($"{BaseUrl}/order/order/{orderIdentifier}/submit"),
                new StringContent(string.Empty, Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task CollectOrder(string orderIdentifier)
        {
            await this._httpClient.PostAsync(new Uri($"{BaseUrl}/order/order/collected"), new StringContent(
                JsonConvert.SerializeObject(new CollectOrderRequest()
                {
                    OrderIdentifier = orderIdentifier
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task<Order> GetOrder(string orderIdentifier)
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/order/order/{orderIdentifier}"))
                .ConfigureAwait(false);

            var order = JsonConvert.DeserializeObject<Order>(await result.Content.ReadAsStringAsync());

            return order;
        }

        private async Task checkRecipeExists(string recipeIdentifier)
        {
            await this._httpClient.PostAsync($"{BaseUrl}/recipes/recipes", new StringContent(
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