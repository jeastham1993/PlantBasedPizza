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
        private List<OrderItem> _items;
        
        [JsonPropertyName("history")]
        private List<OrderHistory> _history;
        
        [JsonConstructor]
        internal Order(string? orderNumber = null)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                orderNumber = Guid.NewGuid().ToString();
            }

            this.OrderIdentifier = "";
            this.CustomerIdentifier = "";
            this.OrderNumber = orderNumber;
            this._items = new List<OrderItem>();
            this._history = new List<OrderHistory>();
        }

        public static Order Create(string orderIdentifier, OrderType type, string customerIdentifier, DeliveryDetails? deliveryDetails = null, string correlationId = "")
        {
            Guard.AgainstNullOrEmpty(customerIdentifier, nameof(customerIdentifier));
            Guard.AgainstNullOrEmpty(orderIdentifier, nameof(orderIdentifier));
            
            if (type == OrderType.Delivery && deliveryDetails == null)
            {
                throw new ArgumentException("If order type is delivery a delivery address must be specified",
                    nameof(deliveryDetails));
            }
            
            ApplicationLogger.Info($"Creating a new order with type {type}");

            var order = new Order()
            {
                OrderType = type,
                OrderIdentifier = orderIdentifier,
                CustomerIdentifier = customerIdentifier,
                OrderDate = DateTime.Now,
                DeliveryDetails = deliveryDetails
            };

            order.AddHistory("Order created");

            DomainEvents.Raise(new OrderCreatedEvent(orderIdentifier)
            {
                CorrelationId = correlationId
            });

            return order;
        }

        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; private set; }
        
        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; private set; }

        [JsonPropertyName("orderDate")]
        public DateTime OrderDate { get; private set; }

        [JsonPropertyName("loyaltyPointsAtOrder")]
        public decimal LoyaltyPointsAtOrder { get; private set; }
        
        [JsonPropertyName("awaitingCollection")]
        public bool AwaitingCollection { get; private set; }

        [JsonPropertyName("orderSubmittedOn")]
        public DateTime? OrderSubmittedOn { get; private set; }

        [JsonPropertyName("orderCompletedOn")]
        public DateTime? OrderCompletedOn { get; private set; }

        [JsonIgnore]
        public IReadOnlyCollection<OrderItem> Items => this._items;
        
        public IReadOnlyCollection<OrderHistory> History()
        {
            return this._history.OrderBy(p => p.HistoryDate).ToList();
        }

        [JsonPropertyName("orderType")]
        public OrderType OrderType { get; private set; }

        [JsonPropertyName("customerIdentifier")]
        public string CustomerIdentifier { get; private set; }

        [JsonPropertyName("totalPrice")]
        public decimal TotalPrice { get; private set; }

        [JsonPropertyName("deliveryDetails")]
        public DeliveryDetails? DeliveryDetails { get; private set; }

        public void AddOrderItem(string recipeIdentifier, string itemName, int quantity, decimal price)
        {
            if (this.OrderSubmittedOn.HasValue)
            {
                ApplicationLogger.Warn("Attempting to add an order item to an order that has already been submitted, skipping");
                return;
            }
            
            if (this._items == null)
            {
                this._items = new List<OrderItem>(1);
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
            if (this._history == null)
            {
                this._history = new List<OrderHistory>(1);
            }
            
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

        public void AddCustomerLoyaltyPoints(decimal pointsAtTimeOfOrder)
        {
            this.LoyaltyPointsAtOrder = pointsAtTimeOfOrder;
        }

        public void SubmitOrder(string correlationId = "")
        {
            if (!this._items.Any())
            {
                throw new ArgumentException("Cannot submit an order with no items");
            }
            
            this.OrderSubmittedOn = DateTime.Now;
            
            this.AddHistory($"Submitted order.");

            DomainEvents.Raise(new OrderSubmittedEvent(OrderIdentifier)
            {
                CorrelationId = correlationId
            }).Wait();
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

            DomainEvents.Raise(new OrderCompletedEvent(this.OrderIdentifier)
            {
                CorrelationId = correlationId
            });
        }
    }
}