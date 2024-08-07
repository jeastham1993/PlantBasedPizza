using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Amazon.EventBridge;
using BackgroundWorkers.IntegrationEvents;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PlantBasedPizza.Events;
using PlantBasedPizza.IntegrationTest.Helpers;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.Orders.IntegrationTest.ViewModels;

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
            
            _eventPublisher = new EventBridgeEventPublisher(new AmazonEventBridgeClient(), Options.Create(new EventBridgeSettings()
            {
                BusName = $"test.orders.{Environment.GetEnvironmentVariable("BUILD_VERSION")}"
            }));
        }

        public async Task SimulateLoyaltyPointsUpdatedEvent(string customerIdentifier, decimal totalPoints)
        {
            await this._eventPublisher.Publish(new CustomerLoyaltyPointsUpdatedEvent()
            {
                CustomerIdentifier = customerIdentifier,
                TotalLoyaltyPoints = totalPoints
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        public async Task SimulateOrderPreparingEvent(string kitchenIdentifier, string orderIdentifier)
        {
            await this._eventPublisher.Publish(new OrderPreparingEventV1()
            {
                KitchenIdentifier = kitchenIdentifier,
                OrderIdentifier = orderIdentifier
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        public async Task SimulateOrderPrepCompleteEvent(string kitchenIdentifier, string orderIdentifier)
        {
            await this._eventPublisher.Publish(new OrderPrepCompleteEventV1()
            {
                KitchenIdentifier = kitchenIdentifier,
                OrderIdentifier = orderIdentifier
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        public async Task SimulatePaymentSuccessEvent(string customerIdentifier, string orderIdentifier)
        {
            await this._eventPublisher.Publish(new PaymentSuccessfulEventV1()
            {
                OrderIdentifier = orderIdentifier,
                CustomerIdentifier = customerIdentifier
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        public async Task SimulateOrderBakedEvent(string kitchenIdentifier, string orderIdentifier)
        {
            await this._eventPublisher.Publish(new OrderBakedEventV1()
            {
                KitchenIdentifier = kitchenIdentifier,
                OrderIdentifier = orderIdentifier
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        public async Task SimulateOrderQualityCheckedEvent(string kitchenIdentifier, string orderIdentifier)
        {
            await this._eventPublisher.Publish(new OrderQualityCheckedEventV1()
            {
                KitchenIdentifier = kitchenIdentifier,
                OrderIdentifier = orderIdentifier
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        public async Task SimulateDriverCollectedEvent(string kitchenIdentifier, string orderIdentifier)
        {
            await this._eventPublisher.Publish(new DriverCollectedOrderEventV1()
            {
                DriverName = "james",
                OrderIdentifier = orderIdentifier
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        public async Task SimulateDriverDeliveredEvent(string kitchenIdentifier, string orderIdentifier)
        {
            await this._eventPublisher.Publish(new DriverDeliveredOrderEventV1()
            {
                OrderIdentifier = orderIdentifier,
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        
        public async Task AddNewDeliveryOrder(string orderIdentifier, string customerIdentifier)
        {
            await this._userHttpClient.PostAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/deliver"), new StringContent(
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
        }

        public async Task<string> AddNewOrder(string customerIdentifier)
        {
            var createdOrder = await this._userHttpClient.PostAsync(new Uri($"{TestConstants.DefaultTestUrl}/order/pickup"), new StringContent(
                JsonConvert.SerializeObject(new CreatePickupOrderCommand()
                {
                    CustomerIdentifier = customerIdentifier
                }), Encoding.UTF8, "application/json")).ConfigureAwait(false);

            var order = await createdOrder.Content.ReadFromJsonAsync<Order>();

            return order.OrderNumber;
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
            await Task.Delay(TimeSpan.FromSeconds(5));
            
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
            // Skip this if running locally, WireMock used instead
            if (TestConstants.DefaultTestUrl.Contains("localhost"))
            {
                return;
            }
            
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