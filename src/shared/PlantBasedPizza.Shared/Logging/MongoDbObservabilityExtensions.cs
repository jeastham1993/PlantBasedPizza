using System.Diagnostics;
using MongoDB.Driver;

namespace PlantBasedPizza.Shared.Logging;

public static class MongoDbObservabilityExtensions
{
    public static void AddToTelemetry(this ReplaceOneResult result)
    {
        if (Activity.Current is null)
            return;

        Activity.Current.AddTag("mongo.matchedCount", result.MatchedCount);
        Activity.Current.AddTag("mongo.modifiedCount", result.ModifiedCount);
    }
}