using System;
using Pulumi.AzureNative.Resources;
using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Authorization;
using Pulumi.AzureNative.ManagedIdentity;
using Pulumi.AzureNative.ServiceBus;
using Pulumi.AzureNative.ServiceBus.Inputs;

return await Pulumi.Deployment.RunAsync(() =>
{
    var config = new Config();
    var ddApiKey = config.RequireSecret("DD_API_KEY");
    var dbConnectionString = config.RequireSecret("DB_CONNECTION_STRING");
    var imageTag = config.Require("IMAGE_TAG");

    // Create an Azure Resource Group
    var resourceGroup = new ResourceGroup("PlantBasedPizza-Pulumi", new ResourceGroupArgs()
    {
        ResourceGroupName = "PlantBasedPizza-Pulumi"
    });

    var serviceBusNamespace = new Namespace("publicsServiceBusNamespace", new NamespaceArgs()
    {
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        NamespaceName = "monolithServiceBus",
        Sku = new SBSkuArgs()
        {
            Name = SkuName.Standard,
            Tier = SkuTier.Standard,
        }
    });

    var namespaceRule = new NamespaceAuthorizationRule("publicServiceBusAccessRule", new NamespaceAuthorizationRuleArgs
    {
        AuthorizationRuleName = "publicServiceBusAccessRule",
        NamespaceName = serviceBusNamespace.Name,
        ResourceGroupName = resourceGroup.Name,
        Rights = new InputList<AccessRights>()
        {
            AccessRights.Manage,
            AccessRights.Listen,
            AccessRights.Send
        }
    });
    
    Console.Write(resourceGroup.GetResourceName());

    var nameSpaceKeys = Output
        .Tuple(serviceBusNamespace.Name, namespaceRule.Name)
        .Apply(t => ListNamespaceKeys.InvokeAsync(new ListNamespaceKeysArgs
        {
            NamespaceName = t.Item1,
            AuthorizationRuleName = t.Item2,
            ResourceGroupName = resourceGroup.GetResourceName()
        }));

    var value = nameSpaceKeys.Apply(key => { return key.PrimaryConnectionString; });

    var publicServiceBusIdentity = new UserAssignedIdentity("serviceBusIdentity", new UserAssignedIdentityArgs()
    {
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        ResourceName = "monolithPubSubIdentity"
    });

    var senderRoleAssignment = new RoleAssignment("senderRoleAssignment", new RoleAssignmentArgs
    {
        PrincipalId = publicServiceBusIdentity.PrincipalId,
        PrincipalType = PrincipalType.ServicePrincipal,
        RoleDefinitionId = "/providers/Microsoft.Authorization/roleDefinitions/69a216fc-b8fb-44d8-bc22-1f3c2cd27a39",
        Scope = serviceBusNamespace.Id
    });

    var receiverRoleAssignment = new RoleAssignment("receiverRoleAssignment", new RoleAssignmentArgs
    {
        PrincipalId = publicServiceBusIdentity.PrincipalId,
        PrincipalType = PrincipalType.ServicePrincipal,
        RoleDefinitionId = "/providers/Microsoft.Authorization/roleDefinitions/4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0",
        Scope = serviceBusNamespace.Id
    });

    var environment = new ManagedEnvironment("managedEnv", new ManagedEnvironmentArgs
    {
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        EnvironmentName = "dev-pulumi",
    });

    var daprComponent = new DaprComponent("pubSubDaprComponent", new DaprComponentArgs
    {
        ComponentName = "public",
        ComponentType = "pubsub.azure.servicebus.topics",
        EnvironmentName = environment.Name,
        IgnoreErrors = false,
        InitTimeout = "50s",
        Metadata = new[]
        {
            new DaprMetadataArgs
            {
                Name = "connectionString",
                Value = value
            },
            new DaprMetadataArgs
            {
                Name = "azureClientId",
                Value = publicServiceBusIdentity.ClientId
            }
        },
        ResourceGroupName = resourceGroup.Name,
        Scopes = new[]
        {
            "monolith",
            "loyalty"
        },
        Version = "v1"
    });

    var containerApp = new ContainerApp("monolith", new ContainerAppArgs
    {
        ContainerAppName = "monolith",
        EnvironmentId = environment.Id,
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        Template = new TemplateArgs()
        {
            Containers = new List<ContainerArgs>()
            {
                new()
                {
                    Name = "application",
                    Image =
                        $"plantpowerjames/plant-based-pizza-monolith:{imageTag}",
                    Resources = new ContainerResourcesArgs()
                    {
                        Cpu = 0.25,
                        Memory = "0.5Gi"
                    },
                    Env = new List<EnvironmentVarArgs>()
                    {
                        new() { Name = "DatabaseConnection", SecretRef = "database-connection" },
                        new() { Name = "Environment", Value = "dev" },
                        new() { Name = "OtlpEndpoint", Value = "http://localhost:4317" }
                    }
                },
                new()
                {
                    Name = "datadog-agent",
                    Image = "index.docker.io/datadog/serverless-init:latest",
                    Resources = new ContainerResourcesArgs()
                    {
                        Cpu = 0.25,
                        Memory = "0.5Gi"
                    },
                    Env = new List<EnvironmentVarArgs>()
                    {
                        new() { Name = "DD_SITE", Value = "datadoghq.eu" },
                        new() { Name = "DD_API_KEY", SecretRef = "dd-api-key" },
                        new() { Name = "DD_ENV", Value = "dev" },
                        new() { Name = "DD_VERSION", Value = imageTag },
                        new() { Name = "DD_SERVICE", Value = "monolith" },
                        new() { Name = "DD_LOGS_ENABLED", Value = "true" },
                        new() { Name = "DD_LOGS_INJECTION", Value = "true" },
                        new()
                        {
                            Name = "DD_APM_IGNORE_RESOURCES",
                            Value = "/opentelemetry.proto.collector.trace.v1.TraceService/Export$"
                        },
                        new() { Name = "DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT", Value = "0.0.0.0:4317" },
                        new() { Name = "DD_AZURE_SUBSCRIPTION_ID", Value = "6d2b072c-7905-4816-b79c-c69bbc5099f3" },
                        new() { Name = "DD_AZURE_RESOURCE_GROUP", Value = resourceGroup.Name }
                    }
                }
            }
        },
        Configuration = new ConfigurationArgs()
        {
            Dapr = new DaprArgs()
            {
                AppPort = 8080,
                AppProtocol = AppProtocol.Http,
                Enabled = true,
                AppId = "monolith"
            },
            Secrets = new List<SecretArgs>()
            {
                new() { Name = "dd-api-key", Value = ddApiKey },
                new()
                {
                    Name = "database-connection", Value = dbConnectionString
                }
            },
            Ingress = new IngressArgs()
            {
                External = true,
                TargetPort = 8080,
                Traffic = new TrafficWeightArgs()
                {
                    Weight = 100,
                    LatestRevision = true
                }
            }
        }
    });

    var loyaltyContainerApp = new ContainerApp("loyalty-service", new ContainerAppArgs
    {
        ContainerAppName = "loyalty",
        EnvironmentId = environment.Id,
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        Template = new TemplateArgs()
        {
            Containers = new List<ContainerArgs>()
            {
                new()
                {
                    Name = "application",
                    Image =
                        $"plantpowerjames/plant-based-pizza-monolith-loyalty:{imageTag}",
                    Resources = new ContainerResourcesArgs()
                    {
                        Cpu = 0.25,
                        Memory = "0.5Gi"
                    },
                    Env = new List<EnvironmentVarArgs>()
                    {
                        new() { Name = "DatabaseConnection", SecretRef = "database-connection" },
                        new() { Name = "Environment", Value = "dev" },
                        new() { Name = "OtlpEndpoint", Value = "http://localhost:4317" }
                    }
                },
                new()
                {
                    Name = "datadog-agent",
                    Image = "index.docker.io/datadog/serverless-init:latest",
                    Resources = new ContainerResourcesArgs()
                    {
                        Cpu = 0.25,
                        Memory = "0.5Gi"
                    },
                    Env = new List<EnvironmentVarArgs>()
                    {
                        new() { Name = "DD_SITE", Value = "datadoghq.eu" },
                        new() { Name = "DD_API_KEY", SecretRef = "dd-api-key" },
                        new() { Name = "DD_ENV", Value = "dev" },
                        new() { Name = "DD_VERSION", Value = imageTag },
                        new() { Name = "DD_SERVICE", Value = "monolith" },
                        new() { Name = "DD_LOGS_ENABLED", Value = "true" },
                        new() { Name = "DD_LOGS_INJECTION", Value = "true" },
                        new()
                        {
                            Name = "DD_APM_IGNORE_RESOURCES",
                            Value = "/opentelemetry.proto.collector.trace.v1.TraceService/Export$"
                        },
                        new() { Name = "DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT", Value = "0.0.0.0:4317" },
                        new() { Name = "DD_AZURE_SUBSCRIPTION_ID", Value = "6d2b072c-7905-4816-b79c-c69bbc5099f3" },
                        new() { Name = "DD_AZURE_RESOURCE_GROUP", Value = resourceGroup.Name }
                    }
                }
            }
        },
        Configuration = new ConfigurationArgs()
        {
            Dapr = new DaprArgs()
            {
                AppPort = 8080,
                AppProtocol = AppProtocol.Http,
                Enabled = true,
                AppId = "loyalty"
            },
            Secrets = new List<SecretArgs>()
            {
                new() { Name = "dd-api-key", Value = ddApiKey },
                new()
                {
                    Name = "database-connection", Value = dbConnectionString
                }
            },
            Ingress = new IngressArgs()
            {
                External = true,
                TargetPort = 8080,
                Traffic = new TrafficWeightArgs()
                {
                    Weight = 100,
                    LatestRevision = true
                }
            }
        }
    });
});