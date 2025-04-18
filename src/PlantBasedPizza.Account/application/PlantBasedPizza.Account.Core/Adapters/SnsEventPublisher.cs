// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;

namespace PlantBasedPizza.Account.Core.Adapters;

public class SnsEventPublisher(AmazonSimpleNotificationServiceClient snsClient, IConfiguration configuration)
    : IEventPublisher
{
    public async Task PublishUserCreated(UserCreatedEventV1 evt)
    {
        await snsClient.PublishAsync(new PublishRequest
        {
            TopicArn = configuration["SNS:UserCreatedTopicArn"],
            Message = JsonSerializer.Serialize(evt)
        });
    }
}