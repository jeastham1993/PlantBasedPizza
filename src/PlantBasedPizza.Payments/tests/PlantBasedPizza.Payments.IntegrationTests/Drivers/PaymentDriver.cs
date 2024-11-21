using Grpc.Core;
using Grpc.Net.Client;

namespace PlantBasedPizza.Payments.IntegrationTests.Drivers;

public class PaymentDriver
    {
        private readonly Payment.PaymentClient _paymentClient;
        private readonly Metadata _apiKeyHeaders;

        public PaymentDriver()
        {
            _apiKeyHeaders = new Metadata
            {
                { "APIKey", "this is a test api key" }
            };
            
            var channel = GrpcChannel.ForAddress(TestConstants.InternalTestEndpoint);
            _paymentClient = new Payment.PaymentClient(channel);
        }

        public async Task<bool> TakePaymentFor(string customerIdentifier, double paymentAmount)
        {
            var res = await _paymentClient.TakePaymentAsync(new TakePaymentRequest()
            {
                CustomerIdentifier = customerIdentifier,
                PaymentAmount = paymentAmount
            }, _apiKeyHeaders);

            return res.IsSuccess;
        }

        public async Task<bool> TakePaymentWithoutAuth(string customerIdentifier, double paymentAmount)
        {
            try
            {
                var res = await _paymentClient.TakePaymentAsync(new TakePaymentRequest()
                {
                    CustomerIdentifier = customerIdentifier,
                    PaymentAmount = paymentAmount,
                    OrderIdentifier = Guid.NewGuid().ToString()
                });

                return res.IsSuccess;
            }
            catch (RpcException)
            {
                return false;
            }
        }
    }