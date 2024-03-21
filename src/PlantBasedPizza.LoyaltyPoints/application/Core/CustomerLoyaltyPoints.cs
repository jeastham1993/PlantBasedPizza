using System.Text.Json.Serialization;

namespace PlantBasedPizza.LoyaltyPoints.Core;

public class CustomerLoyaltyPoints
{
    [JsonConstructor]
    internal CustomerLoyaltyPoints()
    {
        this.CustomerId = "";
        this.History = new List<LoyaltyPointsHistory>();
    }

    public static CustomerLoyaltyPoints Create(string customerIdentifier)
    {
        return new CustomerLoyaltyPoints()
        {
            CustomerId = customerIdentifier,
            TotalPoints = 0,
            History = new List<LoyaltyPointsHistory>()
        };
    }
    
    public string CustomerId { get; set; }

    public decimal TotalPoints { get; set; }
    
    public List<LoyaltyPointsHistory> History { get; set; }

    public void AddLoyaltyPoints(decimal orderValue, string orderIdentifier)
    {
        if (this.History is null)
        {
            this.History = new List<LoyaltyPointsHistory>();
        }

        if (this.History.Any(p => p.OrderIdentifier.Equals(orderIdentifier, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var points = Math.Round(orderValue, 0);

        this.TotalPoints += points;
        this.History.Add(new LoyaltyPointsHistory()
        {
            OrderIdentifier = orderIdentifier,
            DateTime = DateTime.Now,
            OrderValue = orderValue,
            PointsAdded = points
        });
    }

    public void SpendPoints(decimal points, string orderIdentifier)
    {
        if (this.History.Any(p => p.OrderIdentifier.Equals(orderIdentifier, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }
        
        var remainingPoints = this.TotalPoints - points;
        
        if (remainingPoints < 0)
        {
            throw new InsufficientPointsException();
        }

        this.TotalPoints = remainingPoints;
        this.History.Add(new LoyaltyPointsHistory()
        {
            OrderIdentifier = orderIdentifier,
            DateTime = DateTime.Now,
            OrderValue = 0,
            PointsAdded = -remainingPoints
        });
    }
}

public class LoyaltyPointsHistory
{
    public DateTime DateTime { get; set; }
    
    public string OrderIdentifier { get; set; }
    
    public decimal OrderValue { get; set; }
    
    public decimal PointsAdded { get; set; }
}