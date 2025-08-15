// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Payment.Core;
using PlantBasedPizza.Payment.DataTransfer;

namespace PlantBasedPizza.Payment.Infrastructure;

public static class Setup
{
    public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPaymentProvider, NoOpPaymentProvider>();
        services.AddSingleton<TakePaymentCommandHandler>();
        services.AddSingleton<PaymentService>();

        return services;
    }
    
}