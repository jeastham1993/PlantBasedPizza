// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using System.Text.Json.Serialization;

namespace PlantBasedPizza.Kitchen.Core.OrderConfirmed;

public record OrderConfirmed
{
    [JsonPropertyName("OrderIdentifier")]
    public string OrderIdentifier { get; init; }

    public OrderConfirmed(string orderIdentifier)
    {
        if (string.IsNullOrEmpty(orderIdentifier))
        {
            throw new ArgumentException("OrderIdentifier cannot be null or empty");
        }
        
        this.OrderIdentifier = orderIdentifier;
    }
}