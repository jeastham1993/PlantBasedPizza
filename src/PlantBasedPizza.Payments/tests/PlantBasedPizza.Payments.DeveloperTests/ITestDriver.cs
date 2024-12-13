namespace PlantBasedPizza.Payments.InMemoryTests;

public interface ITestDriver
{
    public Task<HttpResponseMessage> TakePaymentWithInvalidBody(string orderIdentifier, decimal amount,
        string? eventId = null);
    
    public Task<HttpResponseMessage> TakePaymentWithInvalidPaymentAmount(string orderIdentifier, string? eventId = null);
    
    public Task<HttpResponseMessage> TakePaymentWithValidBody(string orderIdentifier, decimal amount,
        string? eventId = null);

    public Task<int> VerifySuccessEventReceivedFor(VerificationOptions options);
    
    public Task<int> VerifyFailureEventReceivedFor(VerificationOptions options);
}