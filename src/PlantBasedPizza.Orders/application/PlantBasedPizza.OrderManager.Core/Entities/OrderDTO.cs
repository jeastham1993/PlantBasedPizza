using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.Entities;

public record OrderDto
{
    public OrderDto(Order order)
    {
        this.OrderNumber = order.OrderNumber;
        this.OrderDate = order.OrderDate;
        this.AwaitingCollection = order.AwaitingCollection;
        this.OrderSubmittedOn = order.OrderSubmittedOn;
        this.OrderCompletedOn = order.OrderCompletedOn;
        this.Items = order.Items.Select(item => new OrderItemDto()
        {
            ItemName = item.ItemName,
            Price = item.Price,
            Quantity = item.Quantity,
            RecipeIdentifier = item.RecipeIdentifier
        }).ToList();
        this.History = order.History().Select(history => new OrderHistoryDto()
        {
            Description = history.Description,
            HistoryDate = history.HistoryDate
        }).ToList();
        
        if (order.DeliveryDetails != null)
        {
            this.DeliveryDetails = new DeliveryDetailsDto()
            {
                AddressLine1 = order.DeliveryDetails.AddressLine1,
                AddressLine2 = order.DeliveryDetails.AddressLine2,
                AddressLine3 = order.DeliveryDetails.AddressLine3,
                AddressLine4 = order.DeliveryDetails.AddressLine4,
                AddressLine5 = order.DeliveryDetails.AddressLine5,
                Postcode = order.DeliveryDetails.Postcode,
            };
        }
    }
    
    [JsonPropertyName("orderIdentifier")]
    public string OrderIdentifier { get; set; }
    
    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; }
    
    [JsonPropertyName("deliveryDetails")]
    public DeliveryDetailsDto DeliveryDetails { get; set; }
    
    [JsonPropertyName("orderDate")]
    public DateTime OrderDate { get; set; }
    
    [JsonPropertyName("awaitingCollection")]
    public bool AwaitingCollection { get; set; }
    
    [JsonPropertyName("orderSubmittedOn")]
    public DateTime? OrderSubmittedOn { get; set; }
    
    [JsonPropertyName("orderCompletedOn")]
    public DateTime? OrderCompletedOn { get; set; }
    
    [JsonPropertyName("items")]
    public List<OrderItemDto> Items { get; set; }
        
    [JsonPropertyName("history")]
    public List<OrderHistoryDto> History { get; set; }
}