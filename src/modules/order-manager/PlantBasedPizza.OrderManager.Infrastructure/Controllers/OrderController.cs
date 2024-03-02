using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.OrderManager.Core.Command;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Infrastructure.Controllers
{
    [Route("order")]
    public class OrderController : ControllerBase 
    {
        private readonly IOrderRepository _orderRepository;
        private readonly CollectOrderCommandHandler _collectOrderCommandHandler;
        private readonly AddItemToOrderHandler _addItemToOrderHandler;

        public OrderController(IOrderRepository orderRepository, CollectOrderCommandHandler collectOrderCommandHandler, AddItemToOrderHandler addItemToOrderHandler)
        {
            _orderRepository = orderRepository;
            _collectOrderCommandHandler = collectOrderCommandHandler;
            _addItemToOrderHandler = addItemToOrderHandler;
        }

        /// <summary>
        /// Get the details of a given order.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpGet("order/{orderIdentifier}/detail")]
        public async Task<Order?> Get(string orderIdentifier)
        {
            try
            {
                Activity.Current?.SetTag("orderIdentifier", orderIdentifier);
                
                return await this._orderRepository.Retrieve(orderIdentifier).ConfigureAwait(false);
            }
            catch (OrderNotFoundException)
            {
                this.Response.StatusCode = 404;
                Activity.Current?.AddTag("order.notFound", true);

                return null;
            }
        }

        /// <summary>
        /// Create a new order for pickup.
        /// </summary>
        /// <param name="request">The <see cref="CreatePickupOrderCommand"/> command contents.</param>
        /// <returns></returns>
        [HttpPost("order/pickup")]
        public async Task<Order> Create([FromBody] CreatePickupOrderCommand request)
        {
            var existingOrder = await this._orderRepository.Retrieve(request.OrderIdentifier);

            if (existingOrder != null)
            {
                return existingOrder;
            }
            
            var order = Order.Create(request.OrderIdentifier, request.OrderType, request.CustomerIdentifier, null, CorrelationContext.GetCorrelationId());

            await this._orderRepository.Add(order);

            return order;
        }

        /// <summary>
        /// Create a new delivery order.
        /// </summary>
        /// <param name="request">The <see cref="CreateDeliveryOrder"/> request.</param>
        /// <returns></returns>
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
            }, CorrelationContext.GetCorrelationId());

            await this._orderRepository.Add(order);

            return order;
        }

        /// <summary>
        /// Add an item to the order.
        /// </summary>
        /// <param name="request">the <see cref="AddItemToOrderCommand"/> request.</param>
        /// <returns></returns>
        [HttpPost("order/{orderIdentifier}/items")]
        public async Task<Order?> AddItemToOrder([FromBody] AddItemToOrderCommand request)
        {
            request.AddToTelemetry();

            var order = await this._addItemToOrderHandler.Handle(request);

            if (order is null)
            {
                this.Response.StatusCode = 404;
            }

            return order;
        }
        
        /// <summary>
        /// Submit an order.
        /// </summary>
        /// <param name="orderIdentifier">The order to submit.</param>
        /// <returns></returns>
        [HttpPost("order/{orderIdentifier}/submit")]
        public async Task<Order> SubmitOrder(string orderIdentifier)
        {
            var order = await this._orderRepository.Retrieve(orderIdentifier);

            order.SubmitOrder();

            await this._orderRepository.Update(order);

            return order;
        }

        /// <summary>
        /// List all orders awaiting collection.
        /// </summary>
        /// <returns></returns>
        [HttpGet("order/awaiting-collection")]
        public async Task<List<Order>> GetAwaitingCollection()
        {
            var awaitingCollection = await this._orderRepository.GetAwaitingCollection();

            return awaitingCollection;
        }

        /// <summary>
        /// Mark an order as being collected.
        /// </summary>
        /// <param name="request">The <see cref="CollectOrderRequest"/> request.</param>
        /// <returns></returns>
        [HttpPost("order/collected")]
        public async Task<Order?> OrderCollected([FromBody] CollectOrderRequest request)
        {
            var result = await this._collectOrderCommandHandler.Handle(request);

            if (result is null)
            {
                this.Response.StatusCode = 404;
            }

            return result;
        }
    }
}