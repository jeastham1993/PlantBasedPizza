// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

namespace PlantBasedPizza.OrderManager.Core.MarkAwaitingCollection;

public record MarkAwaitingCollectionCommand
{
    public string OrderIdentifier { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
}