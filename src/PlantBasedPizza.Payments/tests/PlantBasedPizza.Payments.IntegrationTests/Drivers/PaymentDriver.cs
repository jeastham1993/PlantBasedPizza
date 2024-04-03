using Grpc.Net.Client;

namespace PlantBasedPizza.Payments.IntegrationTests.Drivers;

public class PaymentDriver
    {
        private readonly Payment.PaymentClient _paymentClient;

        public PaymentDriver()
        {
            var channel = GrpcChannel.ForAddress(TestConstants.InternalTestEndpoint);
            this._paymentClient = new Payment.PaymentClient(channel);
        }

        public async Task<bool> TakePaymentFor(string customerIdentifier, double paymentAmount)
        {
            var res = await this._paymentClient.TakePaymentAsync(new TakePaymentRequest()
            {
                CustomerIdentifier = customerIdentifier,
                PaymentAmount = paymentAmount
            });

            return res.IsSuccess;
        }
    }