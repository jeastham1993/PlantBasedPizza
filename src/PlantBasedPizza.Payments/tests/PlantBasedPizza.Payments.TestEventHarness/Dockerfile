FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore tests/PlantBasedPizza.Payments.TestEventHarness/PlantBasedPizza.Payments.TestEventHarness.csproj
# Build and publish a release
RUN dotnet publish tests/PlantBasedPizza.Payments.TestEventHarness/PlantBasedPizza.Payments.TestEventHarness.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "PlantBasedPizza.Payments.TestEventHarness.dll"]