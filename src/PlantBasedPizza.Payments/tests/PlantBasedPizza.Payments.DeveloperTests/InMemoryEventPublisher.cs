using PlantBasedPizza.Payments.PublicEvents;

namespace PlantBasedPizza.Payments.InMemoryTests;

public class InMemoryEventPublisher(bool failToPublish = false) : IPaymentEventPublisher
{
    public List<PaymentSuccessfulEventV1> SuccessEvents = new();
    public List<PaymentFailedEventV1> FailedEvents = new();

    public async Task PublishPaymentSuccessfulEventV1(PaymentSuccessfulEventV1 evt)
    {
        if (failToPublish)
        {
            throw new Exception("Simulated failure on event publishing");
        }
        
        SuccessEvents.Add(evt);
    }

    public async Task PublishPaymentFailedEventV1(PaymentFailedEventV1 evt)
    {
        FailedEvents.Add(evt);
    }
}