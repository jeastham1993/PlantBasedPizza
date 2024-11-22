using System.Text.Json.Serialization;
using PlantBasedPizza.Shared.Guards;

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
        internal Order(string? orderIdentifier = null)
        {
            if (string.IsNullOrEmpty(orderIdentifier))
            {
                orderIdentifier = Guid.NewGuid().ToString();
            }

            OrderIdentifier = orderIdentifier;
            CustomerIdentifier = "";
            OrderNumber = orderIdentifier;
            _items = new List<OrderItem>();
            _history = new List<OrderHistory>();
        }

        public static Order Create(OrderType type, string customerIdentifier, DeliveryDetails? deliveryDetails = null, string correlationId = "")
        {
            Guard.AgainstNullOrEmpty(customerIdentifier, nameof(customerIdentifier));
            
            if (type == OrderType.Delivery && deliveryDetails == null)
            {
                throw new ArgumentException("If order type is delivery a delivery address must be specified",
                    nameof(deliveryDetails));
            }
            
            var orderIdentifier = Guid.NewGuid().ToString();

            var order = new Order()
            {
                OrderType = type,
                OrderIdentifier = orderIdentifier,
                OrderNumber = orderIdentifier,
                CustomerIdentifier = customerIdentifier,
                OrderDate = DateTime.Now,
                DeliveryDetails = deliveryDetails
            };

            order.AddHistory("Order created");

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
        public IReadOnlyCollection<OrderItem> Items => _items;
        
        public IReadOnlyCollection<OrderHistory> History()
        {
            return _history.OrderBy(p => p.HistoryDate).ToList();
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
            if (OrderSubmittedOn.HasValue)
            {
                return;
            }
            
            if (_items == null)
            {
                _items = new List<OrderItem>(1);
            }
            
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
            if (OrderSubmittedOn.HasValue)
            {
                return;
            }

            var existingItem = _items.Find(p =>
                p.RecipeIdentifier.Equals(recipeIdentifier, StringComparison.OrdinalIgnoreCase));

            if (existingItem == null)
            {
                return;
            }
            
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
            if (_history == null)
            {
                _history = new List<OrderHistory>(1);
            }
            
            _history.Add(new OrderHistory(description, DateTime.Now));
        }

        public void Recalculate()
        {
            TotalPrice = _items.Sum(p => p.Quantity * p.Price);

            if (OrderType == OrderType.Delivery)
            {
                TotalPrice += DefaultDeliveryPrice;
            }
        }

        public void AddCustomerLoyaltyPoints(decimal pointsAtTimeOfOrder)
        {
            LoyaltyPointsAtOrder = pointsAtTimeOfOrder;
        }

        public void SubmitOrder()
        {
            if (!_items.Any())
            {
                throw new ArgumentException("Cannot submit an order with no items");
            }
            
            OrderSubmittedOn = DateTime.Now;
            
            AddHistory($"Submitted order.");
        }

        public void IsAwaitingCollection()
        {
            AwaitingCollection = true;

            AddHistory("Order awaiting collection");
        }

        public void CompleteOrder()
        {
            OrderCompletedOn = DateTime.Now;
            AwaitingCollection = false;
            
            AddHistory($"Order completed.");
        }
    }
}