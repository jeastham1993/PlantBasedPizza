using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Account.Core;

namespace PlantBasedPizza.Account.Lambda;

/// <summary>
/// Handles DynamoDB stream events for UserAccount data changes
/// </summary>
public class Outbox(ILogger<Outbox> _logger, IEventPublisher _eventPublisher)
{
    /// <summary>
    /// Process DynamoDB stream events for user account changes
    /// </summary>
    /// <param name="dynamoEvent">The DynamoDB stream event</param>
    /// <param name="context">Lambda context</param>
    [LambdaFunction]
    public async Task ProcessStream(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processing {dynamoEvent.Records.Count} DynamoDB records");

        foreach (var record in dynamoEvent.Records)
            try
            {
                var eventName = record.EventName;
                context.Logger.LogInformation($"Event: {eventName}, StreamViewType: {record.Dynamodb.StreamViewType}");

                switch (eventName)
                {
                    case "INSERT":
                        await HandleInsertEvent(record, context);
                        break;
                    case "MODIFY":
                        await HandleModifyEvent(record, context);
                        break;
                    case "REMOVE":
                        await HandleRemoveEvent(record, context);
                        break;
                    default:
                        context.Logger.LogWarning($"Unhandled event type: {eventName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error processing DynamoDB record: {ex.Message}");
                context.Logger.LogError(ex.StackTrace);
                // Continue processing other records even if one fails
            }
    }

    private async Task HandleInsertEvent(DynamoDBEvent.DynamodbStreamRecord record, ILambdaContext context)
    {
        context.Logger.LogInformation("Handling insert event");
        
        // Parse the new image from the stream record
        var userAccount = ParseUserAccountFromDynamoImage(record.Dynamodb.NewImage);

        if (userAccount != null)
        {
            context.Logger.LogInformation(
                $"New user account created: {userAccount.EmailAddress}, Type: {userAccount.AccountType}");

            await _eventPublisher.PublishUserCreated(new UserCreatedEventV1
            {
                AccountId = userAccount.AccountId
            });
            await Task.CompletedTask;
        }
    }

    private async Task HandleModifyEvent(DynamoDBEvent.DynamodbStreamRecord record, ILambdaContext context)
    {
        // Parse both old and new images to identify changes
        var oldUserAccount = ParseUserAccountFromDynamoImage(record.Dynamodb.OldImage);
        var newUserAccount = ParseUserAccountFromDynamoImage(record.Dynamodb.NewImage);

        if (oldUserAccount != null && newUserAccount != null)
        {
            context.Logger.LogInformation($"User account updated: {newUserAccount.EmailAddress}");

            if (oldUserAccount.AccountType != newUserAccount.AccountType)
                context.Logger.LogInformation(
                    $"Account type changed from {oldUserAccount.AccountType} to {newUserAccount.AccountType}");
            // Example: Handle account type change - send notifications, update permissions, etc.
            // Handle other field changes as needed
            await Task.CompletedTask;
        }
    }

    private async Task HandleRemoveEvent(DynamoDBEvent.DynamodbStreamRecord record, ILambdaContext context)
    {
        // Parse the old image (the data that was deleted)
        var userAccount = ParseUserAccountFromDynamoImage(record.Dynamodb.OldImage);

        if (userAccount != null)
        {
            context.Logger.LogInformation(
                $"User account deleted: {userAccount.EmailAddress}, Type: {userAccount.AccountType}");

            // Handle account deletion logic
            // Example: Archive data, clean up related resources, etc.

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Parse DynamoDB attribute values into a UserAccount object
    /// </summary>
    private UserAccount ParseUserAccountFromDynamoImage(
        Dictionary<string, Amazon.Lambda.DynamoDBEvents.DynamoDBEvent.AttributeValue> image)
    {
        if (image == null || !image.ContainsKey("EmailAddress") || !image.ContainsKey("AccountId")) return null;

        try
        {
            var userAccount = new UserAccount
            {
                // PK is the email address in our schema
                EmailAddress = image.ContainsKey("EmailAddress") ? image["EmailAddress"].S : null,
                AccountId = image.ContainsKey("AccountId") ? image["AccountId"].S : null,
                Password = image.ContainsKey("Password") ? image["Password"].S : null
            };

            // Parse the account type
            if (image.ContainsKey("AccountType") && !string.IsNullOrEmpty(image["AccountType"].S))
            {
                if (Enum.TryParse<AccountType>(image["AccountType"].S, out var accountType))
                {
                    userAccount.AccountType = accountType;
                }
                else
                {
                    _logger.LogWarning($"Failed to parse AccountType: {image["AccountType"].S}");
                    userAccount.AccountType = AccountType.User; // Default fallback
                }
            }

            return userAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error parsing UserAccount from DynamoDB: {ex.Message}");
            return null;
        }
    }
}