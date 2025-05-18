// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

namespace PlantBasedPizza.Api;

public static class LambdaSetup
{
    public static IServiceCollection AddLambda(this IServiceCollection services, Serilog.ILogger logger)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LAMBDA_TASK_ROOT")))
        {
            logger.Information("Skipping Lambda configuration");
            return services;
        }
        
        logger.Information("Configuring Lambda hosting");
        
        services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

        if (Environment.GetEnvironmentVariable("PRE_WARM") == "Y")
        {
            logger.Information("Adding before SnapShot requests");

            for (var iterator = 0; iterator < 15; iterator++)
            {
                logger.Information("Iteration: {iterator}", iterator);
                services.AddAWSLambdaBeforeSnapshotRequest(
                    new HttpRequestMessage(HttpMethod.Get, "/recipes"));
                services.AddAWSLambdaBeforeSnapshotRequest(
                    new HttpRequestMessage(HttpMethod.Get, "/health"));
            }
        }
        
        Amazon.Lambda.Core.SnapshotRestore.RegisterBeforeSnapshot(async () =>
        {
            logger.Information("Before snapshot");
            
            
            
            logger.Information("Before snapshot finished");
        });
        
        Amazon.Lambda.Core.SnapshotRestore.RegisterAfterRestore(async () =>
        {
            logger.Information("After restore");
            
            logger.Information("After restore finished");
        });
        
        logger.Information("Finished adding pre-warm requests");

        return services;
    }
}