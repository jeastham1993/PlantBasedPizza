using System.Diagnostics;
using Amazon.Lambda.Core;

namespace BackgroundWorkers;

public static class TraceExtensions
{
    public static void AddToTrace(this ILambdaContext context)
    {
        Activity.Current?.AddTag("cloud.resource_id", context.InvokedFunctionArn);
        Activity.Current?.AddTag("cloud.provider", "aws");
        Activity.Current?.AddTag("cloud.account.id", context.InvokedFunctionArn?.Split(":")[4]);
        Activity.Current?.AddTag("faas.instance", context.LogStreamName);
        Activity.Current?.AddTag("faas.max_memory", context.MemoryLimitInMB);
        Activity.Current?.AddTag("faas.version", context.FunctionVersion);
        Activity.Current?.AddTag("faas.id", context.AwsRequestId);
        
    }
}