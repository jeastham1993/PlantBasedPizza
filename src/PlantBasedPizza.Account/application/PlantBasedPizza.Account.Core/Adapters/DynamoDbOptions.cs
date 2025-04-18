namespace PlantBasedPizza.Account.Core.Adapters;

/// <summary>
/// Configuration options for DynamoDB
/// </summary>
public class DynamoDbOptions
{
    /// <summary>
    /// The DynamoDB table name to use for user accounts
    /// </summary>
    public string TableName { get; set; } = "PlantBasedPizza-Accounts";
} 