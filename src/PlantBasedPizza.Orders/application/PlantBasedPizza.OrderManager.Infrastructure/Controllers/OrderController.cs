using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.OrderSubmitted;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Infrastructure.Controllers
{
    [Route("order")]
    public class OrderController : ControllerBase 
    {
        private readonly IOrderRepository _orderRepository;
        private readonly CollectOrderCommandHandler _collectOrderCommandHandler;
        private readonly AddItemToOrderHandler _addItemToOrderHandler;
        private readonly CreateDeliveryOrderCommandHandler _createDeliveryOrderCommandHandler;
        private readonly CreatePickupOrderCommandHandler _createPickupOrderCommandHandler;
        private readonly SubmitOrderCommandHandler _submitOrderHandler;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderRepository orderRepository, CollectOrderCommandHandler collectOrderCommandHandler, AddItemToOrderHandler addItemToOrderHandler, CreateDeliveryOrderCommandHandler createDeliveryOrderCommandHandler, CreatePickupOrderCommandHandler createPickupOrderCommandHandler, SubmitOrderCommandHandler submitOrderHandler, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _collectOrderCommandHandler = collectOrderCommandHandler;
            _addItemToOrderHandler = addItemToOrderHandler;
            _createDeliveryOrderCommandHandler = createDeliveryOrderCommandHandler;
            _createPickupOrderCommandHandler = createPickupOrderCommandHandler;
            _submitOrderHandler = submitOrderHandler;
            _logger = logger;
        }

        /// <summary>
        /// Get all the orders for the current customer.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpGet("")]
        [Authorize(Roles = "user")]
        public async Task<IEnumerable<OrderDto>> GetForCustomer()
        {
            try
            {
                var accountId = User.Claims.ExtractAccountId();

                var orders = await _orderRepository.ForCustomer(accountId);
                
                return orders.Select(order => new OrderDto(order));
            }
            catch (Exception)
            {
                Response.StatusCode = 400;

                return new List<OrderDto>();
            }
        }

        /// <summary>
        /// Get the details of a given order.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpGet("{orderIdentifier}/detail")]
        [Authorize(Roles = "user")]
        public async Task<OrderDto?> Get(string orderIdentifier)
        {
            try
            {
                var accountId = User.Claims.ExtractAccountId();
                
                Activity.Current?.SetTag("orderIdentifier", orderIdentifier);
                
                var order = await _orderRepository.Retrieve(orderIdentifier).ConfigureAwait(false);

                if (order.CustomerIdentifier != accountId)
                {
                    throw new OrderNotFoundException(orderIdentifier);
                }
                
                return new OrderDto(order);
            }
            catch (OrderNotFoundException)
            {
                Response.StatusCode = 404;
                Activity.Current?.AddTag("order.notFound", true);

                return null;
            }
        }

        /// <summary>
        /// Create a new order for pickup.
        /// </summary>
        /// <param name="request">The <see cref="CreatePickupOrderCommand"/> command contents.</param>
        /// <returns></returns>
        [HttpPost("pickup")]
        [Authorize(Roles = "user")]
        public async Task<OrderDto?> Create([FromBody] CreatePickupOrderCommand request)
        {
            request.CustomerIdentifier = User.Claims.ExtractAccountId();
            
            return await _createPickupOrderCommandHandler.Handle(request);
        }

        /// <summary>
        /// Create a new delivery order.
        /// </summary>
        /// <param name="request">The <see cref="CreateDeliveryOrder"/> request.</param>
        /// <returns></returns>
        [HttpPost("deliver")]
        [Authorize(Roles = "user")]
        public async Task<OrderDto?> Create([FromBody] CreateDeliveryOrder request)
        {
            request.CustomerIdentifier = User.Claims.ExtractAccountId();
            
            return await _createDeliveryOrderCommandHandler.Handle(request);   
        }

        /// <summary>
        /// Add an item to the order.
        /// </summary>
        /// <param name="request">the <see cref="AddItemToOrderCommand"/> request.</param>
        /// <returns></returns>
        [HttpPost("{orderIdentifier}/items")]
        [Authorize(Roles = "user")]
        public async Task<OrderDto?> AddItemToOrder([FromBody] AddItemToOrderCommand request)
        {
            request.AddToTelemetry();
            request.CustomerIdentifier = User.Claims.ExtractAccountId();

            var order = await _addItemToOrderHandler.Handle(request);

            if (order is null)
            {
                Response.StatusCode = 404;
            }

            return new OrderDto(order);
        }
        
        /// <summary>
        /// Submit an order.
        /// </summary>
        /// <param name="orderIdentifier">The order to submit.</param>
        /// <returns></returns>
        [HttpPost("{orderIdentifier}/submit")]
        [Authorize(Roles = "user")]
        public async Task<OrderDto?> SubmitOrder(string orderIdentifier)
        {
            var accountId = User.Claims.ExtractAccountId();

            try
            {
                var result = await this._submitOrderHandler.Handle(new SubmitOrderCommand()
                {
                    OrderIdentifier = orderIdentifier,
                    CustomerIdentifier = accountId
                });

                Response.StatusCode = 201;
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failure submitting order");
                
                Response.StatusCode = 500;
                return null;
            }
        }

        /// <summary>
        /// List all orders awaiting collection.
        /// </summary>
        /// <returns></returns>
        [HttpGet("awaiting-collection")]
        [Authorize(Roles = "staff")]
        public async Task<IEnumerable<OrderDto>> GetAwaitingCollection()
        {
            var awaitingCollection = await _orderRepository.GetAwaitingCollection();

            return awaitingCollection.Select(order => new OrderDto(order));
        }

        /// <summary>
        /// Mark an order as being collected.
        /// </summary>
        /// <param name="request">The <see cref="CollectOrderRequest"/> request.</param>
        /// <returns></returns>
        [HttpPost("collected")]
        [Authorize(Roles = "staff")]
        public async Task<OrderDto?> OrderCollected([FromBody] CollectOrderRequest request) =>
            await _collectOrderCommandHandler.Handle(request);
    }
}