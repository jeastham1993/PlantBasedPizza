using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Guards;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Core.Entities;

public class Order
{
    private const decimal DefaultDeliveryPrice = 3.50M;

    [JsonIgnore]
    [NotMapped]
    private List<IntegrationEvent> _events = new();

    [JsonProperty("items")] private List<OrderItem> _items;

    [JsonProperty("history")] private List<OrderHistory> _history;

    [JsonConstructor]
    internal Order(string? orderNumber = null)
    {
        if (string.IsNullOrEmpty(orderNumber)) orderNumber = Guid.NewGuid().ToString();

        OrderIdentifier = "";
        CustomerIdentifier = "";
        OrderNumber = orderNumber;
        _items = new List<OrderItem>();
        _history = new List<OrderHistory>();
        _events = new List<IntegrationEvent>();
    }

    public static Order Create(string orderIdentifier, OrderType type, string customerIdentifier,
        DeliveryDetails? deliveryDetails = null, string correlationId = "")
    {
        Guard.AgainstNullOrEmpty(customerIdentifier, nameof(customerIdentifier));
        Guard.AgainstNullOrEmpty(orderIdentifier, nameof(orderIdentifier));

        if (type == OrderType.Delivery && deliveryDetails == null)
            throw new ArgumentException("If order type is delivery a delivery address must be specified",
                nameof(deliveryDetails));

        ApplicationLogger.Info($"Creating a new order with type {type}");

        var order = new Order()
        {
            OrderType = type,
            OrderIdentifier = orderIdentifier,
            CustomerIdentifier = customerIdentifier,
            OrderDate = DateTime.Now.ToUniversalTime(),
            DeliveryDetails = deliveryDetails
        };

        order.AddHistory("Order created");

        DomainEvents.Raise(new OrderCreatedEvent(orderIdentifier)
        {
            CorrelationId = correlationId
        });

        return order;
    }

    [JsonProperty] public string OrderIdentifier { get; private set; }

    [JsonProperty] public string OrderNumber { get; private set; }

    [JsonProperty] public DateTime OrderDate { get; private set; }

    [JsonProperty] public bool AwaitingCollection { get; private set; }

    [JsonProperty] public DateTime? OrderSubmittedOn { get; private set; }

    [JsonProperty] public DateTime? OrderCompletedOn { get; private set; }

    [JsonIgnore] public IReadOnlyCollection<OrderItem> Items => _items;

    [JsonIgnore]
    [NotMapped]
    public IReadOnlyCollection<IntegrationEvent> Events => (_events ??  new());

    [JsonIgnore]
    public IReadOnlyCollection<OrderHistory> History => _history.OrderBy(p => p.HistoryDate).ToList();

    [JsonProperty]
    public OrderType OrderType { get; private set; }

    [JsonProperty] public string CustomerIdentifier { get; private set; }

    [JsonProperty] public decimal TotalPrice { get; private set; }

    [JsonProperty] public DeliveryDetails? DeliveryDetails { get; private set; }

    public void AddOrderItem(string recipeIdentifier, string itemName, int quantity, decimal price)
    {
        if (OrderSubmittedOn.HasValue)
        {
            ApplicationLogger.Warn(
                "Attempting to add an order item to an order that has already been submitted, skipping");
            return;
        }

        if (_items == null) _items = new List<OrderItem>(1);

        var existingItem = _items.Find(p =>
            p.RecipeIdentifier.Equals(recipeIdentifier, StringComparison.OrdinalIgnoreCase));

        if (existingItem != null)
        {
            quantity += existingItem.Quantity;
            _items.Remove(existingItem);
        }

        AddHistory($"Added {quantity} {itemName} to order.");

        _items.Add(new OrderItem(recipeIdentifier, itemName, quantity, price));

        Recalculate();
    }

    public void RemoveOrderItem(string recipeIdentifier, int quantity)
    {
        if (OrderSubmittedOn.HasValue) return;

        var existingItem = _items.Find(p =>
            p.RecipeIdentifier.Equals(recipeIdentifier, StringComparison.OrdinalIgnoreCase));

        if (existingItem == null) return;

        AddHistory($"Removing {quantity} {existingItem.ItemName} from order.");

        _items.Remove(existingItem);

        if (existingItem.Quantity - quantity <= 0)
        {
            Recalculate();

            return;
        }

        _items.Add(new OrderItem(recipeIdentifier, existingItem.ItemName, existingItem.Quantity - quantity,
            existingItem.Price));

        Recalculate();
    }

    public void AddHistory(string description)
    {
        if (_history == null) _history = new List<OrderHistory>(1);

        _history.Add(new OrderHistory(description, DateTime.Now.ToUniversalTime()));
    }

    public void Recalculate()
    {
        TotalPrice = _items.Sum(p => p.Quantity * p.Price);

        if (OrderType == OrderType.Delivery) TotalPrice += DefaultDeliveryPrice;
    }

    public void SubmitOrder(string correlationId = "")
    {
        if (!_items.Any()) throw new ArgumentException("Cannot submit an order with no items");

        OrderSubmittedOn = DateTime.Now.ToUniversalTime();

        AddHistory($"Submitted order.");

        DomainEvents.Raise(new OrderSubmittedEvent(OrderIdentifier)
        {
            CorrelationId = correlationId
        }).Wait();
    }

    public void IsAwaitingCollection(string correlationId = "")
    {
        AwaitingCollection = true;

        AddHistory("Order awaiting collection");
    }

    public void CompleteOrder(string correlationId = "")
    {
        OrderCompletedOn = DateTime.Now.ToUniversalTime();
        AwaitingCollection = false;

        AddHistory($"Order completed.");

        var evt = new OrderCompletedEvent(CustomerIdentifier, OrderIdentifier, TotalPrice)
        {
            CorrelationId = correlationId
        };

        DomainEvents.Raise(evt).GetAwaiter().GetResult();
        addIntegrationEvent(evt);
    }

    private void addIntegrationEvent(IntegrationEvent evt)
    {
        if (_events is null) _events = new List<IntegrationEvent>();

        _events.Add(evt);
    }
}