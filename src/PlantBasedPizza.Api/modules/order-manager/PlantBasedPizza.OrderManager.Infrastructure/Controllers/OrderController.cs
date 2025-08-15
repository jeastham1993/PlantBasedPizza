using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.Configuration;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.SubmitOrder;

namespace PlantBasedPizza.OrderManager.Infrastructure.Controllers;

[Route("order")]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly CollectOrderCommandHandler _collectOrderCommandHandler;
    private readonly AddItemToOrderHandler _addItemToOrderHandler;
    private readonly CreateDeliveryOrderCommandHandler _createDeliveryOrderCommandHandler;
    private readonly CreatePickupOrderCommandHandler _createPickupOrderCommandHandler;
    private readonly SubmitOrderCommandHandler _submitOrderCommandHandler;
    private readonly IValidator<CreatePickupOrderCommand> _createPickupOrderValidator;
    private readonly IValidator<CreateDeliveryOrderCommand> _createDeliveryOrderValidator;
    private readonly IValidator<AddItemToOrderCommand> _addItemToOrderValidator;
    private readonly OrderOptions _orderOptions;

    public OrderController(IOrderRepository orderRepository, CollectOrderCommandHandler collectOrderCommandHandler,
        AddItemToOrderHandler addItemToOrderHandler,
        CreateDeliveryOrderCommandHandler createDeliveryOrderCommandHandler,
        CreatePickupOrderCommandHandler createPickupOrderCommandHandler,
        SubmitOrderCommandHandler submitOrderCommandHandler,
        IValidator<CreatePickupOrderCommand> createPickupOrderValidator,
        IValidator<CreateDeliveryOrderCommand> createDeliveryOrderValidator,
        IValidator<AddItemToOrderCommand> addItemToOrderValidator,
        IOptions<OrderOptions> orderOptions)
    {
        _orderRepository = orderRepository;
        _collectOrderCommandHandler = collectOrderCommandHandler;
        _addItemToOrderHandler = addItemToOrderHandler;
        _createDeliveryOrderCommandHandler = createDeliveryOrderCommandHandler;
        _createPickupOrderCommandHandler = createPickupOrderCommandHandler;
        _submitOrderCommandHandler = submitOrderCommandHandler;
        _createPickupOrderValidator = createPickupOrderValidator ?? throw new ArgumentNullException(nameof(createPickupOrderValidator));
        _createDeliveryOrderValidator = createDeliveryOrderValidator ?? throw new ArgumentNullException(nameof(createDeliveryOrderValidator));
        _addItemToOrderValidator = addItemToOrderValidator ?? throw new ArgumentNullException(nameof(addItemToOrderValidator));
        _orderOptions = orderOptions?.Value ?? throw new ArgumentNullException(nameof(orderOptions));
    }

    /// <summary>
    /// Get the details of a given order.
    /// </summary>
    /// <param name="orderIdentifier">The order identifier.</param>
    /// <returns></returns>
    [HttpGet("{orderIdentifier}/detail")]
    public async Task<OrderDto?> Get(string orderIdentifier)
    {
        try
        {
            Activity.Current?.SetTag("orderIdentifier", orderIdentifier);

            var order = await _orderRepository.Retrieve(orderIdentifier).ConfigureAwait(false);

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
    public async Task<ActionResult<OrderDto>> CreatePickupOrder([FromBody] CreatePickupOrderCommand request)
    {
        var validationResult = await _createPickupOrderValidator.ValidateAsync(request);
        
        if (!validationResult.IsValid)
        {
            var problemDetails = new ValidationProblemDetails();
            foreach (var error in validationResult.Errors)
            {
                problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
            }
            return BadRequest(problemDetails);
        }
        
        var result = await _createPickupOrderCommandHandler.Handle(request);
        return result is not null ? Ok(result) : BadRequest("Failed to create pickup order");
    }

    /// <summary>
    /// Create a new delivery order.
    /// </summary>
    /// <param name="request">The <see cref="CreateDeliveryOrder"/> request.</param>
    /// <returns></returns>
    [HttpPost("deliver")]
    public async Task<ActionResult<OrderDto>> CreateDeliveryOrder([FromBody] CreateDeliveryOrderCommand request)
    {
        var validationResult = await _createDeliveryOrderValidator.ValidateAsync(request);
        
        if (!validationResult.IsValid)
        {
            var problemDetails = new ValidationProblemDetails();
            foreach (var error in validationResult.Errors)
            {
                problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
            }
            return BadRequest(problemDetails);
        }
        
        var result = await _createDeliveryOrderCommandHandler.Handle(request);
        return result is not null ? Ok(result) : BadRequest("Failed to create delivery order");
    }

    /// <summary>
    /// Add an item to the order.
    /// </summary>
    /// <param name="request">the <see cref="AddItemToOrderCommand"/> request.</param>
    /// <returns></returns>
    [HttpPost("{orderIdentifier}/items")]
    public async Task<ActionResult<OrderDto>> AddItemToOrder([FromBody] AddItemToOrderCommand request)
    {
        var validationResult = await _addItemToOrderValidator.ValidateAsync(request);
        
        if (!validationResult.IsValid)
        {
            var problemDetails = new ValidationProblemDetails();
            foreach (var error in validationResult.Errors)
            {
                problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
            }
            return BadRequest(problemDetails);
        }

        request.AddToTelemetry();

        var order = await _addItemToOrderHandler.Handle(request);

        return order is not null ? Ok(order) : NotFound();
    }

    /// <summary>
    /// Submit an order.
    /// </summary>
    /// <param name="orderIdentifier">The order to submit.</param>
    /// <returns></returns>
    [HttpPost("{orderIdentifier}/submit")]
    public async Task<OrderDto?> SubmitOrder([FromBody] SubmitOrderCommand request)
    {
        request.AddToTelemetry();

        var order = await _submitOrderCommandHandler.Handle(request);

        if (order is null) Response.StatusCode = 404;

        return order;
    }


    /// <summary>
    /// List all orders awaiting collection.
    /// </summary>
    /// <returns></returns>
    [HttpGet("awaiting-collection")]
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
    public async Task<OrderDto?> OrderCollected([FromBody] CollectOrderRequest request)
    {
        return await _collectOrderCommandHandler.Handle(request);
    }
}