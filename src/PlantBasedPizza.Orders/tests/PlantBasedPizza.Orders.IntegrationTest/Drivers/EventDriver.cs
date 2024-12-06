using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dapr.Client;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantBasedPizza.IntegrationTest.Helpers;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.DriverDeliveredOrder;
using PlantBasedPizza.OrderManager.Core.OrderQualityChecked;
using PlantBasedPizza.OrderManager.Core.PaymentSuccess;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Orders.IntegrationTest.ViewModels;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.IntegrationTest.Drivers;

public class EventDriver
{
    private readonly DaprClient _daprClient;
    private readonly IDeadLetterRepository _deadLetterRepository;
    private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";
    
    public EventDriver()
    {
        _daprClient = new DaprClientBuilder()
            .UseGrpcEndpoint("http://localhost:40003")
            .Build();

        BsonClassMap.RegisterClassMap<DeadLetterMessage>(map =>
        {
            map.AutoMap();
            map.SetIgnoreExtraElements(true);
            map.SetIgnoreExtraElementsIsInherited(true);
        });
        _deadLetterRepository = new DeadLetterRepository(new MongoClient(TestConstants.DefaultMongoDbConnection));
    }

    public async Task SimulateLoyaltyPointsUpdatedEvent(string customerIdentifier, decimal totalPoints,
        string? eventId = null)
    {
        await _daprClient.PublishEventAsync("public", "loyalty.customerLoyaltyPointsUpdated.v1",
            new CustomerLoyaltyPointsUpdatedEvent
            {
                CustomerIdentifier = customerIdentifier,
                TotalLoyaltyPoints = totalPoints
            }, new Dictionary<string, string>(1)
            {
                { "cloudevent.id", eventId ?? Guid.NewGuid().ToString() },
                { "cloudevent.type", "loyalty.customerLoyaltyPointsUpdated.v1" },
                { "cloudevent.source", "loyalty" },
                { "cloudevent.time", DateTime.UtcNow.ToString(DATE_FORMAT) },
            });

        // Delay to allow for message processing
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    public async Task SimulatePaymentSuccessEvent(string orderIdentifier, decimal paymentValue, string? eventId = null)
    {
        await _daprClient.PublishEventAsync("payments", "payments.paymentSuccessful.v1", new PaymentSuccessfulEventV1
        {
            OrderIdentifier = orderIdentifier,
            Amount = paymentValue
        }, new Dictionary<string, string>(1)
        {
            { "cloudevent.id", eventId ?? Guid.NewGuid().ToString() },
            { "cloudevent.type", "payments.paymentSuccessful.v1" },
            { "cloudevent.source", "payments" },
            { "cloudevent.time", DateTime.UtcNow.ToString(DATE_FORMAT) },
        });

        // Delay to allow for message processing
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    public async Task SimulateInvalidPaymentSuccessEvent()
    {
        await _daprClient.PublishEventAsync("payments", "payments.paymentSuccessful.v1", new InvalidPaymentSuccessEvent
        {
            OrderId = "twetwt",
            Money = 12.99M
        }, new Dictionary<string, string>(1)
        {
            { "cloudevent.id", Guid.NewGuid().ToString() },
            { "cloudevent.type", "payments.paymentSuccessful.v1" },
            { "cloudevent.source", "payments" },
            { "cloudevent.time", DateTime.UtcNow.ToString(DATE_FORMAT) },
        });

        // Delay to allow for message processing
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    public async Task SimulateQualityCheckCompleteEvent(string orderIdentifier, string? eventId = null)
    {
        await _daprClient.PublishEventAsync("public", "kitchen.qualityChecked.v1", new OrderQualityCheckedEventV1
        {
            OrderIdentifier = orderIdentifier
        }, new Dictionary<string, string>(1)
        {
            { "cloudevent.id", eventId ?? Guid.NewGuid().ToString() },
            { "cloudevent.type", "kitchen.qualityChecked.v1" },
            { "cloudevent.source", "kitchen" },
            { "cloudevent.time", DateTime.UtcNow.ToString(DATE_FORMAT) },
        });

        // Delay to allow for message processing
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    public async Task SimulateOrderDeliveredEvent(string orderIdentifier, string? eventId = null)
    {
        await _daprClient.PublishEventAsync("public", "delivery.driverDeliveredOrder.v1",
            new DriverDeliveredOrderEventV1
            {
                OrderIdentifier = orderIdentifier
            }, new Dictionary<string, string>(1)
            {
                { "cloudevent.id", eventId ?? Guid.NewGuid().ToString() },
                { "cloudevent.type", "delivery.driverDeliveredOrder.v1" },
                { "cloudevent.source", "delivery" },
                { "cloudevent.time", DateTime.UtcNow.ToString(DATE_FORMAT) },
            });

        // Delay to allow for message processing
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    public async Task<DeadLetterMessage?> VerifyEventReachesDeadLetterInbox(string eventId)
    {
        var deadLetterMessages = await _deadLetterRepository.GetDeadLetterItems();
        
        var specificMessage = deadLetterMessages.FirstOrDefault(x => x.EventId == eventId);
        
        return specificMessage;
    }
}

record InvalidPaymentSuccessEvent
{
    public string OrderId { get; set; }
    
    public decimal Money { get; set; }
}