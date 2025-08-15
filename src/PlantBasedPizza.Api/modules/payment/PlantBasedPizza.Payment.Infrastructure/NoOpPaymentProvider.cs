using PlantBasedPizza.Payment.Core;

namespace PlantBasedPizza.Payment.Infrastructure;

public class NoOpPaymentProvider : IPaymentProvider
{
    public async Task<string> TakePaymentAsync(decimal amount)
    {
        await Task.Delay(new Random().Next(0, 100));

        return Guid.NewGuid().ToString();
    }
}