// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using System.Collections.Immutable;
using Aspire.Hosting.Dapr;

namespace PlantBasedPizza.Monolith;

public static class ServiceExtensions
{
    public static IResourceBuilder<ContainerResource> AddLocalObservability(this IDistributedApplicationBuilder builder)
    {
        var jaeger = builder.AddContainer("jaeger", "jaegertracing/opentelemetry-all-in-one")
            .WithEndpoint(16686, 16686, "http", name: "ui")
            .WithEndpoint(13133, 13133, "http", name: "random")
            .WithEndpoint(4317, 4317, "http", name: "otel-grpc")
            .WithEndpoint(4318, 4318, "http", name: "otel-http");

        return jaeger;
    }
    
    public static IResourceBuilder<ContainerResource> AddDaprWithScheduler(this IDistributedApplicationBuilder builder)
    {
        var scheduler = builder.AddContainer("scheduler", "daprio/dapr")
            .WithEndpoint(50006, 50006)
            .WithArgs("./scheduler", "--port", "50006", "--etcd-data-dir", "/var/lock")
            .WithVolume("/var/local");

        builder.AddDapr()
            .AddDaprComponent("public", "pubsub", new DaprComponentOptions()
            {
                LocalPath = "../../components/pubsub.yaml"
            })
            .WaitFor(scheduler);

        return scheduler;
    }

    public static IResourceBuilder<ContainerResource> AddMockedLoyaltyService(
        this IDistributedApplicationBuilder builder)
    {
        var loyaltyService = builder.AddContainer("loyalty", "wiremock/wiremock")
            .WithHttpEndpoint(8080, 8080, name: "http")
            .WithBindMount("../../src/PlantBasedPizza.Api/mocks/loyalty-api", "/home/wiremock")
            .WithDaprSidecar(new DaprSidecarOptions()
            {
                AppId = "loyalty",
                ResourcesPaths = new[] { "../../components/" }.ToImmutableHashSet()
            });

        return loyaltyService;
    }

    public static IResourceBuilder<ProjectResource> AddApplication(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<MongoDBDatabaseResource> db,
        IResourceBuilder<ContainerResource> observability)
    {
        var monolith = builder.AddProject<Projects.PlantBasedPizza_Api>("monolith")
            .WithDaprSidecar(new DaprSidecarOptions()
            {
                AppId = "monolith",
                ResourcesPaths = new[] { "../../components/" }.ToImmutableHashSet()
            })
            .WithEnvironment("DatabaseConnection", db.Resource.ConnectionStringExpression)
            .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", observability.GetEndpoint("otel-grpc"))
            .WithReference(db)
            .WaitFor(observability);

        return monolith;
    }
}