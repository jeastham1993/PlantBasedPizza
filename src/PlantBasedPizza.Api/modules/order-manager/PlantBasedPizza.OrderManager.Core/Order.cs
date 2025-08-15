using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.OrderManager.Core;

public class Order
{
    private const decimal DefaultDeliveryPrice = 3.50M;

    [JsonIgnore]
    [NotMapped]
    private List<IntegrationEvent> _events = new();

    [JsonPropertyName("items")] private List<OrderItem> _items = new();

    [JsonPropertyName("history")] private List<OrderHistory> _history = new();

    [JsonConstructor]
    internal Order(string? orderNumber = null)
    {
        if (string.IsNullOrEmpty(orderNumber)) orderNumber = Guid.NewGuid().ToString();

        OrderIdentifier = "";
        CustomerIdentifier = "";
        OrderNumber = orderNumber;
        // Collections already initialized above
        _events = new List<IntegrationEvent>();
    }

    // Internal constructor for factory
    internal Order(string orderIdentifier, OrderType orderType, string customerIdentifier, DeliveryDetails? deliveryDetails = null)
    {
        Guard.AgainstNullOrEmpty(orderIdentifier, nameof(orderIdentifier));
        Guard.AgainstNullOrEmpty(customerIdentifier, nameof(customerIdentifier));

        if (orderType == OrderType.Delivery && deliveryDetails == null)
            throw new ArgumentException("Delivery details are required for delivery orders");

        OrderIdentifier = orderIdentifier;
        OrderType = orderType;
        CustomerIdentifier = customerIdentifier;
        OrderDate = DateTime.UtcNow;
        OrderNumber = Guid.NewGuid().ToString();
        DeliveryDetails = deliveryDetails;
        _events = new List<IntegrationEvent>();
        
        AddHistory("Order created.");
    }

    // Static Create method removed - use IOrderFactory instead

    [JsonPropertyName("orderIdentifier")] public string OrderIdentifier { get; private set; }

    [JsonPropertyName("orderNumber")] public string OrderNumber { get; private set; }

    [JsonPropertyName("orderDate")] public DateTime OrderDate { get; private set; }

    [JsonPropertyName("awaitingCollection")] public bool AwaitingCollection { get; private set; }

    [JsonPropertyName("orderSubmittedOn")] public DateTime? OrderSubmittedOn { get; private set; }

    [JsonPropertyName("orderCompletedOn")] public DateTime? OrderCompletedOn { get; private set; }

    [JsonIgnore] public IReadOnlyCollection<OrderItem> Items => _items;

    [JsonIgnore]
    [NotMapped]
    public IReadOnlyCollection<IntegrationEvent> Events => (_events ??  new());

    [JsonIgnore]
    public IReadOnlyCollection<OrderHistory> History => _history.OrderBy(p => p.HistoryDate).ToList();

    [JsonPropertyName("orderType")]
    public OrderType OrderType { get; private set; }

    [JsonPropertyName("customerIdentifier")] public string CustomerIdentifier { get; private set; }

    [JsonPropertyName("totalPrice")] public decimal TotalPrice { get; private set; }

    [JsonPropertyName("deliveryDetails")] public DeliveryDetails? DeliveryDetails { get; private set; }

    public void AddOrderItem(string recipeIdentifier, string itemName, int quantity, decimal price)
    {
        if (OrderSubmittedOn.HasValue)
        {
            Activity.Current?.AddTag("order.submitted", true);
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

        _history.Add(new OrderHistory(description, DateTime.UtcNow));
    }

    public void Recalculate()
    {
        TotalPrice = _items.Sum(p => p.Quantity * p.Price);

        if (OrderType == OrderType.Delivery) TotalPrice += DefaultDeliveryPrice;
    }

    public void MarkAsSubmitted()
    {
        OrderSubmittedOn = DateTime.UtcNow;
    }

    public void MarkAsAwaitingCollection()
    {
        AwaitingCollection = true;
        AddHistory("Order is awaiting collection.");
    }

    public void MarkAsCompleted()
    {
        OrderCompletedOn = DateTime.UtcNow;
        AwaitingCollection = false;
    }

    public void AddIntegrationEvent(IntegrationEvent evt)
    {
        if (_events is null) _events = new List<IntegrationEvent>();
        _events.Add(evt);
    }

}