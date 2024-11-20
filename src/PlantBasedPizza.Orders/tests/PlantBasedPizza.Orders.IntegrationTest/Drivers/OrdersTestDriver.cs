using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using PlantBasedPizza.Events;
using PlantBasedPizza.IntegrationTest.Helpers;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.Orders.IntegrationTest.ViewModels;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.IntegrationTest.Drivers;

public class OrdersTestDriver
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly HttpClient _userHttpClient;
        private readonly HttpClient _staffHttpClient;

        public OrdersTestDriver()
        {
            var userToken = TestTokenGenerator.GenerateTestTokenForRole("user");
            var staffToken = TestTokenGenerator.GenerateTestTokenForRole("staff");
            
            _userHttpClient = new HttpClient();
            _userHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            
            _staffHttpClient = new HttpClient();
            _staffHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", staffToken);
            
            // _eventPublisher = new RabbitMQEventPublisher(new OptionsWrapper<RabbitMqSettings>(new RabbitMqSettings()
            // {
            //     ExchangeName = "dev.plantbasedpizza",
            //     HostName = "localhost"
            // }), new Logger<RabbitMQEventPublisher>(new SerilogLoggerFactory()),  new RabbitMQConnection("localhost"));
        }

        public async Task SimulateLoyaltyPointsUpdatedEvent(string customerIdentifier, decimal totalPoints)
        {
            await this._eventPublisher.Publish(new CustomerLoyaltyPointsUpdatedEvent()
            {
                CustomerIdentifier = customerIdentifier,
                TotalLoyaltyPoints = totalPoints
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
        
        public async Task AddNewDeliveryOrder(string orderIdentifier, string customerIdentifier)
        {
            await this._userHttpClient.PostAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/deliver"), new StringContent(
                JsonConvert.SerializeObject(new CreateDeliveryOrder()
                {
                    OrderIdentifier = orderIdentifier,
                    CustomerIdentifier = customerIdentifier,
                    AddressLine1 = "My test address",
                    AddressLine2 = string.Empty,
                    AddressLine3 = string.Empty,
                    AddressLine4 = string.Empty,
                    AddressLine5 = string.Empty,
                    Postcode = "TYi9PO"
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task AddNewOrder(string orderIdentifier, string customerIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            await this._userHttpClient.PostAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/pickup"), new StringContent(
                JsonConvert.SerializeObject(new CreatePickupOrderCommand()
                {
                    OrderIdentifier = orderIdentifier,
                    CustomerIdentifier = customerIdentifier
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task AddItemToOrder(string orderIdentifier, string recipeIdentifier, int quantity)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            await checkRecipeExists(recipeIdentifier).ConfigureAwait(false);

            await this._userHttpClient.PostAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/{orderIdentifier}/items"),
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
            await this._userHttpClient.PostAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/{orderIdentifier}/submit"),
                new StringContent(string.Empty, Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task CollectOrder(string orderIdentifier)
        {
            // Delay to allow async processing to catch up
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            var res = await this._staffHttpClient.PostAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/collected"), new StringContent(
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
            var result = await this._userHttpClient.GetAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/{orderIdentifier}/detail"))
                .ConfigureAwait(false);

            var order = JsonConvert.DeserializeObject<Order>(await result.Content.ReadAsStringAsync());

            return order;
        }

        private async Task checkRecipeExists(string recipeIdentifier)
        {
            await this._userHttpClient.PostAsync($"{TestConstants.DefaultTestUrl}/recipes", new StringContent(
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