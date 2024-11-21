using System.Text.Json.Serialization;

namespace PlantBasedPizza.LoyaltyPoints.Shared.Core;

public class CustomerLoyaltyPoints
{
    [JsonConstructor]
    internal CustomerLoyaltyPoints()
    {
        CustomerId = "";
        History = new List<LoyaltyPointsHistory>();
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
        if (History is null)
        {
            History = new List<LoyaltyPointsHistory>();
        }

        if (History.Exists(p => p.OrderIdentifier.Equals(orderIdentifier, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var points = Math.Round(orderValue, 0);

        TotalPoints += points;
        History.Add(new LoyaltyPointsHistory()
        {
            OrderIdentifier = orderIdentifier,
            DateTime = DateTime.Now,
            OrderValue = orderValue,
            PointsAdded = points
        });
    }

    public void SpendPoints(decimal points, string orderIdentifier)
    {
        if (History.Exists(p => p.OrderIdentifier.Equals(orderIdentifier, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }
        
        var remainingPoints = TotalPoints - points;
        
        if (remainingPoints < 0)
        {
            throw new InsufficientPointsException();
        }

        TotalPoints = remainingPoints;
        History.Add(new LoyaltyPointsHistory()
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