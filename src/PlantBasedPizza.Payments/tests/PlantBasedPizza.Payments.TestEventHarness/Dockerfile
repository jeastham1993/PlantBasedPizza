FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /App

# Copy everything
COPY ./shared ./shared
COPY ./PlantBasedPizza.Payments ./PlantBasedPizza.Payments
# Restore as distinct layers
RUN dotnet restore PlantBasedPizza.Payments/tests/PlantBasedPizza.Payments.TestEventHarness/PlantBasedPizza.Payments.TestEventHarness.csproj
# Build and publish a release
RUN dotnet publish PlantBasedPizza.Payments/tests/PlantBasedPizza.Payments.TestEventHarness/PlantBasedPizza.Payments.TestEventHarness.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled as runtime
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "PlantBasedPizza.Payments.TestEventHarness.dll"]