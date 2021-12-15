using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Command;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure.Controllers
{
    [Route("order")]
    public class OrderController : ControllerBase 
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRecipeService _recipeService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderRepository orderRepository, IRecipeService recipeService, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _recipeService = recipeService;
            _logger = logger;
        }

        [HttpGet("order/{orderIdentifier}")]
        public async Task<Order> Get(string orderIdentifier)
        {
            return await this._orderRepository.Retrieve(orderIdentifier).ConfigureAwait(false);
        }

        [HttpPost("order/pickup")]
        public async Task<Order> Create([FromBody] CreatePickupOrderCommand request)
        {
            var existingOrder = await this._orderRepository.Retrieve(request.OrderIdentifier);

            if (existingOrder != null)
            {
                return existingOrder;
            }
            
            var order = Order.Create(request.OrderIdentifier, request.OrderType, request.CustomerIdentifier, null, this.Request.Headers["CorrelationId"]);

            await this._orderRepository.Add(order);

            return order;
        }

        [HttpPost("order/deliver")]
        public async Task<Order> Create([FromBody] CreateDeliveryOrder request)
        {
            var existingOrder = await this._orderRepository.Retrieve(request.OrderIdentifier);

            if (existingOrder != null)
            {
                return existingOrder;
            }

            var order = Order.Create(request.OrderIdentifier, request.OrderType, request.CustomerIdentifier, new DeliveryDetails()
            {
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                AddressLine3 = request.AddressLine3,
                AddressLine4 = request.AddressLine4,
                AddressLine5 = request.AddressLine5,
                Postcode = request.Postcode,
            }, this.Request.Headers["CorrelationId"]);

            await this._orderRepository.Add(order);

            return order;
        }

        [HttpPost("order/{orderIdentifier}/items")]
        public async Task<Order> AddItemToOrder([FromBody] AddItemToOrderCommand request)
        {
            var recipe = await this._recipeService.GetRecipe(request.RecipeIdentifier);
            
            var order = await this._orderRepository.Retrieve(request.OrderIdentifier);

            order.AddOrderItem(request.RecipeIdentifier, recipe.ItemName, request.Quantity, recipe.Price);

            await this._orderRepository.Update(order);

            return order;
        }

        [HttpPost("order/{orderIdentifier}/submit")]
        public async Task<Order> SubmitOrder(string orderIdentifier)
        {
            var order = await this._orderRepository.Retrieve(orderIdentifier);

            order.SubmitOrder();

            await this._orderRepository.Update(order);

            return order;
        }

        [HttpGet("order/awaiting-collection")]
        public async Task<List<Order>> GetAwaitingCollection()
        {
            var awaitingCollection = await this._orderRepository.GetAwaitingCollection();

            return awaitingCollection;
        }

        [HttpPost("order/collected")]
        public async Task<Order> OrderCollected([FromBody] CollectOrderRequest request)
        {
            this._logger.LogInformation($"Received {request}");
            
            var existingOrder = await this._orderRepository.Retrieve(request.OrderIdentifier);

            if (existingOrder == null)
            {
                this._logger.LogInformation("Existing order not found, returning");
                
                return existingOrder;
            }
            
            this._logger.LogInformation($"Order is type {existingOrder.OrderType} and is awaiting collection {existingOrder.AwaitingCollection}");

            if (existingOrder.OrderType == OrderType.DELIVERY || existingOrder.AwaitingCollection == false)
            {
                this._logger.LogInformation("Returning");
                
                return existingOrder;
            }

            existingOrder.AddHistory("Order collected");

            existingOrder.CompleteOrder();

            await this._orderRepository.Update(existingOrder).ConfigureAwait(false);

            return existingOrder;
        }
    }
}