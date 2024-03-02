namespace PlantBasedPizza.Shared.Logging;

public static class CorrelationContext
{
    private static readonly AsyncLocal<string> _correlationId = new AsyncLocal<string>();

    private const string __defaultRequestHeaderName = "X-Correlation-ID";

    public static string DefaultRequestHeaderName => __defaultRequestHeaderName;
    
    public static void SetCorrelationId(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation Id cannot be null or empty", nameof(correlationId));
        }

        if (!string.IsNullOrWhiteSpace(_correlationId.Value))
        {
            throw new InvalidOperationException("Correlation Id is already set for the context");
        }

        _correlationId.Value = correlationId;
    }

    public static string GetCorrelationId()
    {
        return _correlationId.Value ?? string.Empty;
    }
}