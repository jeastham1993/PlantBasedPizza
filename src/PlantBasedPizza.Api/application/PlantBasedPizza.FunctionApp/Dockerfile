FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

COPY . /src/dotnet-function-app
RUN cd /src/dotnet-function-app && \
mkdir -p /home/site/wwwroot && \
dotnet publish *.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH="/home/site/wwwroot/datadog/linux-x64/Datadog.Trace.ClrProfiler.Native.so"
ENV DD_DOTNET_TRACER_HOME="/home/site/wwwroot/datadog"
ENV DD_TRACE_HTTP_CLIENT_EXCLUDED_URL_SUBSTRINGS = "monitor.azure, applicationinsights.azure, metadata/instance/compute, admin/host, AzureFunctionsRpcMessages.FunctionRpc"


COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]