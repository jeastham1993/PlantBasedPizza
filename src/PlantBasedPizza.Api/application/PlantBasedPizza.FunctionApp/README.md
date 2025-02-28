# Deploying Azure Function

1. Create the function app

```sh
az functionapp create --functions-version 4 --name plantbaseddockeronpremium  --os-type linux --runtime dotnet-isolated --storage-account ${STORAGE_ACCOUNT_NAME} --image plantpowerjames/plantbasedpizza-function-app:latest --https-only true --plan ${APP_SERVICE_PREMIUM_PLAN_NAME} --resource-group ${RG_NAME}
```

2. Post deployment, navigate to the Azure Console and open your function app
3. Navigate to Settings > Environment Variables and add the following:

```
DD_API_KEY=
DD_SITE=
DD_SERVICE=
DD_ENV=
DD_VERSION=
CORECLR_ENABLE_PROFILING=1
CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
CORECLR_PROFILER_PATH="/home/site/wwwroot/datadog/linux-x64/Datadog.Trace.ClrProfiler.Native.so"
DD_DOTNET_TRACER_HOME="/home/site/wwwroot/datadog"
DD_TRACE_HTTP_CLIENT_EXCLUDED_URL_SUBSTRINGS = "monitor.azure, applicationinsights.azure, metadata/instance/compute, admin/host, AzureFunctionsRpcMessages.FunctionRpc"

```