using PlantBasedPizza.Payment.Core;

namespace PlantBasedPizza.Payment.DataTransfer;

public class PaymentService(TakePaymentCommandHandler commandHandler)
{
    public async Task<PaymentResultDTO> TakePayment(TakePaymentCommand command)
    {
        var result = await commandHandler.Handle(command);

        return new PaymentResultDTO(result);
    }
}
