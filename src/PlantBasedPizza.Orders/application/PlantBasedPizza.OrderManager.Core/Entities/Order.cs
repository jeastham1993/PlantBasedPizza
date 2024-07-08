using System.Text.Json.Serialization;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Guards;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Core.Entities
{
    public class Order
    {
        private const decimal DefaultDeliveryPrice = 3.50M;

        [JsonPropertyName("items")]
        public List<OrderItem> _items { get; init; }
        
        [JsonPropertyName("history")]
        public List<OrderHistory> _history { get; init; }
        
        [JsonConstructor]
        internal Order(string orderNumber = null)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                orderNumber = Guid.NewGuid().ToString();
            }
            
            this.CustomerIdentifier = "";
            this.OrderNumber = orderNumber;
            this._items = new List<OrderItem>();
            this._history = new List<OrderHistory>();
        }

        public static Order Create(OrderType type, string customerIdentifier, DeliveryDetails? deliveryDetails = null)
        {
            Guard.AgainstNullOrEmpty(customerIdentifier, nameof(customerIdentifier));
            
            if (type == OrderType.Delivery && deliveryDetails == null)
            {
                throw new ArgumentException("If order type is delivery a delivery address must be specified",
                    nameof(deliveryDetails));
            }

            var order = new Order()
            {
                OrderType = type,
                OrderNumber = Guid.NewGuid().ToString(),
                CustomerIdentifier = customerIdentifier,
                OrderDate = DateTime.Now,
                DeliveryDetails = deliveryDetails
            };

            order.AddHistory("Order created");

            return order;
        }
        
        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; init; }

        [JsonPropertyName("orderDate")]
        public DateTime OrderDate { get; init; }

        [JsonPropertyName("loyaltyPointsAtOrder")]
        public decimal LoyaltyPointsAtOrder { get; init; }
        
        [JsonPropertyName("awaitingCollection")]
        public bool AwaitingCollection { get; set; }

        [JsonPropertyName("orderSubmittedOn")]
        public DateTime? OrderSubmittedOn { get; set; }

        [JsonPropertyName("orderCompletedOn")]
        public DateTime? OrderCompletedOn { get; set; }

        [JsonIgnore]
        public IReadOnlyCollection<OrderItem> Items => this._items;
        
        public IReadOnlyCollection<OrderHistory> History()
        {
            return this._history.OrderBy(p => p.HistoryDate).ToList();
        }

        [JsonPropertyName("orderType")]
        public OrderType OrderType { get; init; }

        [JsonPropertyName("customerIdentifier")]
        public string CustomerIdentifier { get; init; }

        [JsonPropertyName("totalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("deliveryDetails")]
        public DeliveryDetails? DeliveryDetails { get; init; }

        public void AddOrderItem(string recipeIdentifier, string itemName, int quantity, decimal price)
        {
            if (this.OrderSubmittedOn.HasValue)
            {
                return;
            }
            
            var existingItem = this._items.Find(p =>
                p.RecipeIdentifier.Equals(recipeIdentifier, StringComparison.OrdinalIgnoreCase));

            if (existingItem != null)
            {
                quantity += existingItem.Quantity;
                this._items.Remove(existingItem);
            }
            
            this.AddHistory($"Added {quantity} {itemName} to order.");

            this._items.Add(new OrderItem(recipeIdentifier, itemName, quantity, price));

            this.Recalculate();
        }

        public void RemoveOrderItem(string recipeIdentifier, int quantity)
        {
            if (this.OrderSubmittedOn.HasValue)
            {
                return;
            }

            var existingItem = this._items.Find(p =>
                p.RecipeIdentifier.Equals(recipeIdentifier, StringComparison.OrdinalIgnoreCase));

            if (existingItem == null)
            {
                return;
            }
            
            this.AddHistory($"Removing {quantity} {existingItem.ItemName} from order.");

            this._items.Remove(existingItem);

            if (existingItem.Quantity - quantity <= 0)
            {
                this.Recalculate();

                return;
            }

            this._items.Add(new OrderItem(recipeIdentifier, existingItem.ItemName, existingItem.Quantity - quantity,
                existingItem.Price));

            this.Recalculate();
        }

        public void AddHistory(string description)
        {
            this._history.Add(new OrderHistory(description, DateTime.Now));
        }

        public void Recalculate()
        {
            this.TotalPrice = this._items.Sum(p => p.Quantity * p.Price);

            if (this.OrderType == OrderType.Delivery)
            {
                this.TotalPrice += DefaultDeliveryPrice;
            }
        }

        public void SubmitOrder(string correlationId = "")
        {
            if (!this._items.Any())
            {
                throw new ArgumentException("Cannot submit an order with no items");
            }
            
            this.OrderSubmittedOn = DateTime.Now;
            
            this.AddHistory($"Submitted order.");
        }

        public void IsAwaitingCollection(string correlationId = "")
        {
            this.AwaitingCollection = true;

            this.AddHistory("Order awaiting collection");
        }

        public void CompleteOrder(string correlationId = "")
        {
            this.OrderCompletedOn = DateTime.Now;
            this.AwaitingCollection = false;
            
            this.AddHistory($"Order completed.");
        }
    }
}