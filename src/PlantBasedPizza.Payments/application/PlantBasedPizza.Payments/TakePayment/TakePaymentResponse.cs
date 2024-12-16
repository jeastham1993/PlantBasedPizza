namespace PlantBasedPizza.Payments.TakePayment;

public enum TakePaymentStatus
{
    OK,
    PAYMENT_FAILED,
    UNEXPECTED_ERROR
}

public record TakePaymentResponse(TakePaymentStatus Status);