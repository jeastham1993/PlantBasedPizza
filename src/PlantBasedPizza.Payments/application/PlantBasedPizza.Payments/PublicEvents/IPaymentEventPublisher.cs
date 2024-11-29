namespace PlantBasedPizza.Payments.PublicEvents;

public interface IPaymentEventPublisher
{
    Task PublishPaymentSuccessfulEventV1(PaymentSuccessfulEventV1 evt);
    
    Task PublishPaymentFailedEventV1(PaymentFailedEventV1 evt);
}