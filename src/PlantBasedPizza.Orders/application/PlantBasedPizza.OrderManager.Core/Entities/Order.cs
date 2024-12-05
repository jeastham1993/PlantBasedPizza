using System.Text.Json.Serialization;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.PublicEvents;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.OrderManager.Core.Entities
{
    public class Order
    {
        private const decimal DefaultDeliveryPrice = 3.50M;

        [JsonIgnore]
        private List<IntegrationEvent> _events;

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
            _events = new List<IntegrationEvent>();
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
            order._events.Add(new OrderCreatedEventV1()
            {
                OrderIdentifier = orderIdentifier,
                OrderValue = 0
            });
            
            order._events.Add(new OrderCreatedEventV2()
            {
                OrderId = orderIdentifier,
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

        [JsonPropertyName("orderCancelledOn")]
        public DateTime? OrderCancelledOn { get; private set; }

        [JsonIgnore]
        public IReadOnlyCollection<OrderItem> Items => _items;
        
        public IReadOnlyCollection<OrderHistory> History()
        {
            return _history.OrderBy(p => p.HistoryDate).ToList();
        }

        [JsonIgnore]
        public IReadOnlyCollection<IntegrationEvent> Events
        {
            get
            {
                if (_events == null)
                {
                    _events = [];
                }
                
                return _events;
            }
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

        public void Recalculate()
        {
            TotalPrice = _items.Sum(p => p.Quantity * p.Price);

            if (OrderType == OrderType.Delivery)
            {
                TotalPrice += DefaultDeliveryPrice;
            }
        }

        public void Confirm(decimal paymentAmount)
        {
            if (OrderCancelledOn.HasValue)
            {
                return;
            }
            
            AddHistory($"Payment taken for {paymentAmount}!");
            AddHistory("Order confirmed");
            
            addEvent(new OrderConfirmedEventV1()
            {
                OrderIdentifier = OrderIdentifier,
            });
        }

        public void ReadyForDelivery()
        {
            if (OrderType != OrderType.Delivery || DeliveryDetails == null)
            {
                return;
            }
            
            AddHistory("Sending for delivery");

            addEvent(new OrderReadyForDeliveryEventV1()
            {
                OrderIdentifier = OrderIdentifier,
                DeliveryAddressLine1 = DeliveryDetails.AddressLine1,
                DeliveryAddressLine2 = DeliveryDetails.AddressLine2,
                DeliveryAddressLine3 = DeliveryDetails.AddressLine3,
                DeliveryAddressLine4 = DeliveryDetails.AddressLine4,
                DeliveryAddressLine5 = DeliveryDetails.AddressLine5,
                Postcode = DeliveryDetails.Postcode,
            });
        }
        
        public void SubmitOrder()
        {
            if (!_items.Any())
            {
                throw new ArgumentException("Cannot submit an order with no items");
            }

            if (OrderCancelledOn.HasValue)
            {
                return;
            }
            
            OrderSubmittedOn = DateTime.Now;
            
            AddHistory($"Submitted order.");
            addEvent(new OrderSubmittedEventV1()
            {
                OrderIdentifier = OrderIdentifier,
            });
        }

        public void IsAwaitingCollection()
        {
            AwaitingCollection = true;

            AddHistory("Order awaiting collection");
        }

        public bool CancelOrder()
        {
            if (OrderSubmittedOn.HasValue)
            {
                return false;
            }
            
            AddHistory("Order cancelled");
            OrderCancelledOn = DateTime.Now;
            
            addEvent(new OrderCancelledEventV1()
            {
                OrderIdentifier = OrderIdentifier
            });

            return true;
        }

        public void CompleteOrder()
        {
            OrderCompletedOn = DateTime.Now;
            AwaitingCollection = false;
            
            AddHistory($"Order completed.");
            
            addEvent(new OrderCompletedIntegrationEventV1()
            {
                OrderIdentifier = OrderIdentifier,
                CustomerIdentifier = CustomerIdentifier,
                OrderValue = TotalPrice,
            });
            
            addEvent(new OrderCompletedIntegrationEventV2()
            {
                OrderIdentifier = OrderIdentifier,
                CustomerIdentifier = CustomerIdentifier,
                OrderValue = new OrderValue()
                {
                    Value = TotalPrice,
                    Currency = "GBP"
                }
            });
        }
        
        public void AddHistory(string description)
        {
            if (_history == null)
            {
                _history = new List<OrderHistory>(1);
            }
            
            _history.Add(new OrderHistory(description, DateTime.Now));
        }

        private void addEvent(IntegrationEvent evt)
        {
            if (_events is null)
            {
                _events = new List<IntegrationEvent>();
            }
            
            _events.Add(evt);
        }
    }
}