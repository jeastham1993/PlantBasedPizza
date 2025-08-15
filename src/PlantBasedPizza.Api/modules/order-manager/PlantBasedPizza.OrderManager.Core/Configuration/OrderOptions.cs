namespace PlantBasedPizza.OrderManager.Core.Configuration;

public class OrderOptions
{
    public const string SectionName = "OrderSettings";
    
    public decimal DefaultDeliveryPrice { get; set; } = 3.50m;
    public int MaxItemsPerOrder { get; set; } = 20;
    public TimeSpan OrderTimeoutMinutes { get; set; } = TimeSpan.FromMinutes(30);
    public string DefaultCurrency { get; set; } = "GBP";
    public bool EnableOrderTracking { get; set; } = true;
}