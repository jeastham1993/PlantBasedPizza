using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.Infrastructure.Extensions;
using PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;

namespace PlantBasedPizza.OrderManager.Infrastructure.Controllers
{
    [Route("order")]
    public class OrderController : ControllerBase 
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentService _paymentService;
        private readonly IOrderEventPublisher _eventPublisher;
        private readonly ILoyaltyPointService _loyaltyPointService;
        private readonly CollectOrderCommandHandler _collectOrderCommandHandler;
        private readonly AddItemToOrderHandler _addItemToOrderHandler;
        private readonly CreateDeliveryOrderCommandHandler _createDeliveryOrderCommandHandler;
        private readonly CreatePickupOrderCommandHandler _createPickupOrderCommandHandler;

        public OrderController(IOrderRepository orderRepository, CollectOrderCommandHandler collectOrderCommandHandler, AddItemToOrderHandler addItemToOrderHandler, CreateDeliveryOrderCommandHandler createDeliveryOrderCommandHandler, CreatePickupOrderCommandHandler createPickupOrderCommandHandler, IPaymentService paymentService, ILoyaltyPointService loyaltyPointService, IOrderEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _collectOrderCommandHandler = collectOrderCommandHandler;
            _addItemToOrderHandler = addItemToOrderHandler;
            _createDeliveryOrderCommandHandler = createDeliveryOrderCommandHandler;
            _createPickupOrderCommandHandler = createPickupOrderCommandHandler;
            _paymentService = paymentService;
            _loyaltyPointService = loyaltyPointService;
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
                
                Activity.Current?.SetTag("orderIdentifier", orderIdentifier);
                
                var order = await this._orderRepository.Retrieve(orderIdentifier).ConfigureAwait(false);

                if (order.CustomerIdentifier != accountId)
                {
                    throw new OrderNotFoundException(orderIdentifier);
                }
                
                return new OrderDto(order);
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
        public async Task<OrderDto?> AddItemToOrder([FromBody] AddItemToOrderCommand request)
        {
            request.AddToTelemetry();
            request.CustomerIdentifier = User.Claims.ExtractAccountId();

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
            
            var order = await this._orderRepository.Retrieve(orderIdentifier);
            
            if (order.CustomerIdentifier != accountId)
            {
                throw new OrderNotFoundException(orderIdentifier);
            }

            await this._paymentService.TakePaymentFor(order);
            var loyaltyPoints = await this._loyaltyPointService.GetCustomerLoyaltyPoints(order.CustomerIdentifier);
            
            order.AddCustomerLoyaltyPoints(loyaltyPoints);
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
        public async Task<OrderDto?> OrderCollected([FromBody] CollectOrderRequest request) =>
            await this._collectOrderCommandHandler.Handle(request);
    }
}