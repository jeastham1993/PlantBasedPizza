namespace PlantBasedPizza.Deliver.Core.Configuration;

public class DeliveryOptions
{
    public const string SectionName = "DeliverySettings";
    
    public decimal DeliveryFee { get; set; } = 3.50m;
    public int MaxDeliveryDistanceKm { get; set; } = 10;
    public TimeSpan EstimatedDeliveryTime { get; set; } = TimeSpan.FromMinutes(30);
    public int MaxConcurrentDeliveries { get; set; } = 5;
    public bool EnableDriverTracking { get; set; } = true;
}