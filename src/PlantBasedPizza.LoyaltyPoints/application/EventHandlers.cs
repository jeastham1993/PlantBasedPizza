// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using Dapr;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Core;

namespace PlantBasedPizza.LoyaltyPoints;

public class EventHandlers
{
    private const string OrderCompletedEventName = "order.orderCompleted.v1";

    [Topic("public",
        OrderCompletedEventName)]
    public static async Task<IResult> HandleOrderCompletedEvent(
        [FromServices] AddLoyaltyPointsCommandHandler handler,
        HttpContext httpContext,
        OrderCompletedEvent evt)
    {
        await handler.Handle(new AddLoyaltyPointsCommand()
        {
            OrderValue = evt.OrderValue,
            OrderIdentifier = evt.OrderIdentifier,
            CustomerIdentifier = evt.CustomerIdentifier
        });

        return Results.Ok();
    }
}