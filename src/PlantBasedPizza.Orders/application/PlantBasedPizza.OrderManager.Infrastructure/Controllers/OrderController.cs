using System.Diagnostics;
using Datadog.Trace;
using Datadog.Trace.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.Infrastructure.Extensions;

namespace PlantBasedPizza.OrderManager.Infrastructure.Controllers
{
    [Route("order")]
    [EnableCors("CorsPolicy")]
    public class OrderController : ControllerBase 
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEventPublisher _eventPublisher;
        private readonly CollectOrderCommandHandler _collectOrderCommandHandler;
        private readonly AddItemToOrderHandler _addItemToOrderHandler;
        private readonly CreateDeliveryOrderCommandHandler _createDeliveryOrderCommandHandler;
        private readonly CreatePickupOrderCommandHandler _createPickupOrderCommandHandler;

        public OrderController(IOrderRepository orderRepository, CollectOrderCommandHandler collectOrderCommandHandler, AddItemToOrderHandler addItemToOrderHandler, CreateDeliveryOrderCommandHandler createDeliveryOrderCommandHandler, CreatePickupOrderCommandHandler createPickupOrderCommandHandler, IOrderEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _collectOrderCommandHandler = collectOrderCommandHandler;
            _addItemToOrderHandler = addItemToOrderHandler;
            _createDeliveryOrderCommandHandler = createDeliveryOrderCommandHandler;
            _createPickupOrderCommandHandler = createPickupOrderCommandHandler;
            _eventPublisher = eventPublisher;
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
                    
                Tracer.Instance.ActiveScope?.Span.SetTag("orderIdentifier", orderIdentifier);
                    
                var order = await this._orderRepository.Retrieve(accountId, orderIdentifier).ConfigureAwait(false);
                    
                return new OrderDto(order);
            }
            catch (OrderNotFoundException)
            {
                this.Response.StatusCode = 404;
                Tracer.Instance.ActiveScope?.Span.SetTag("order.notFound", "true");

                return null;
            }
        }
        
        

        /// <summary>
        /// Get the details of a given order.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpGet("")]
        [Authorize(Roles = "user")]
        public async Task<IEnumerable<OrderDto>> Get()
        {
            try
            {
                var accountId = User.Claims.ExtractAccountId();
                    
                var orders = await this._orderRepository.RetrieveCustomerOrders(accountId).ConfigureAwait(false);
                    
                return orders.Select(ord => new OrderDto(ord));
            }
            catch (Exception)
            {
                this.Response.StatusCode = 400;

                return new List<OrderDto>();
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
            
            return await this._createPickupOrderCommandHandler.Handle(request);
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
            
            return await this._createDeliveryOrderCommandHandler.Handle(request);   
        }

        /// <summary>
        /// Add an item to the order.
        /// </summary>
        /// <param name="request">the <see cref="AddItemToOrderCommand"/> request.</param>
        /// <returns></returns>
        [HttpPost("{orderIdentifier}/items")]
        [Authorize(Roles = "user")]
        public async Task<OrderDto?> AddItemToOrder(string orderIdentifier, [FromBody] AddItemToOrderCommand request)
        {
            request.AddToTelemetry();
            
            request.CustomerIdentifier = User.Claims.ExtractAccountId();
            request.OrderIdentifier = orderIdentifier;

            var order = await this._addItemToOrderHandler.Handle(request);

            if (order is null)
            {
                this.Response.StatusCode = 404;
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
        public async Task<OrderDto> SubmitOrder(string orderIdentifier)
        {
            var accountId = User.Claims.ExtractAccountId();
            
            var order = await this._orderRepository.Retrieve(accountId, orderIdentifier);

            order.SubmitOrder();

            await this._orderRepository.Update(order);
            await this._eventPublisher.PublishOrderSubmittedEventV1(order);

            return new OrderDto(order);
        }

        /// <summary>
        /// List all orders awaiting collection.
        /// </summary>
        /// <returns></returns>
        [HttpGet("awaiting-collection")]
        [Authorize(Roles = "staff")]
        public async Task<IEnumerable<OrderDto>> GetAwaitingCollection()
        {
            var awaitingCollection = await this._orderRepository.GetAwaitingCollection();

            return awaitingCollection.Select(order => new OrderDto(order));
        }

        /// <summary>
        /// Mark an order as being collected.
        /// </summary>
        /// <param name="request">The <see cref="CollectOrderRequest"/> request.</param>
        /// <returns></returns>
        [HttpPost("collected")]
        [Authorize(Roles = "staff")]
        public async Task<OrderDto?> OrderCollected([FromBody] CollectOrderRequest request)
        {
            var accountId = User.Claims.ExtractAccountId();
            request.CustomerIdentifier = accountId;
            
            return await this._collectOrderCommandHandler.Handle(request);
        }
    }
}