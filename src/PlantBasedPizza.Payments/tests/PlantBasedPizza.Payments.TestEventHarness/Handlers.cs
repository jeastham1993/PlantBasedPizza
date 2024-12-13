using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Payments.TestEventHarness.PublicEvents;

namespace PlantBasedPizza.Payments.TestEventHarness;

public record HandlerLogger
{
}

public static class Handlers
{
    private const string PaymentSuccessfulEventName = "payments.paymentSuccessful.v1";
    private const string PaymentFailedEventName = "payments.paymentFailed.v1";
    private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

    [Topic("payments",
        PaymentSuccessfulEventName)]
    public static async Task<IResult> HandlePaymentSuccessfulEvent(
        [FromServices] InMemoryEventMonitor eventMonitor,
        [FromServices] ILogger<HandlerLogger> logger,
        PaymentSuccessfulEventV1 evt)
    {
        try
        {
            logger.LogInformation("Received success event");
            eventMonitor.Add(new ReceivedEvent
            {
                EntityId = evt.OrderIdentifier,
                EventName = nameof(PaymentSuccessfulEventV1)
            });

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failure processing");
            return Results.InternalServerError();
        }
    }

    [Topic("payments",
        PaymentFailedEventName)]
    public static async Task<IResult> HandlePaymentFailedEvent(
        [FromServices] InMemoryEventMonitor eventMonitor,
        [FromServices] ILogger<HandlerLogger> logger,
        PaymentFailedEventV1 evt)
    {
        try
        {
            logger.LogInformation("Received failed event");
            eventMonitor.Add(new ReceivedEvent
            {
                EntityId = evt.OrderIdentifier,
                EventName = nameof(PaymentFailedEventV1)
            });

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failure processing");
            return Results.InternalServerError();
        }
    }

    public static async Task<IEnumerable<ReceivedEvent>> GetEventsForEntity(
        [FromServices] InMemoryEventMonitor eventMonitor,
        string orderIdentifier)
    {
        var events = eventMonitor.GetEvent(orderIdentifier);

        return events;
    }

    public static async Task<IResult> SendTakePaymentCommand(
        [FromServices] DaprClient daprClient,
        [FromServices] ILogger<HandlerLogger> logger,
        [FromBody] TakePaymentCommand command)
    {
        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_ID_HEADER_KEY, Guid.NewGuid().ToString() },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DATE_FORMAT) }
        };

        logger.LogInformation("Publishing {orderIdentifier} for {amount}", command.OrderIdentifier,
            command.PaymentAmount);
        await daprClient.PublishEventAsync("payments", "payments.takepayment.v1", command, eventMetadata);

        return Results.Ok();
    }
}