// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

namespace PlantBasedPizza.Payment.Core;

public class TakePaymentCommandHandler(IPaymentProvider provider)
{
    public async Task<PaymentResult> Handle(TakePaymentCommand command)
    {
        if (command.Amount <= 0)
        {
            return new  PaymentResult()
            {
                Message = "Payment amount must be greater than zero.",
                PaymentId = string.Empty
            };
        }

        var paymentResult = await provider.TakePaymentAsync(command.Amount);

        return new PaymentResult()
        {
            Message = "OK",
            PaymentId = paymentResult
        };
    }
}