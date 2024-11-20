FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Worker/PlantBasedPizza.Orders.Worker.csproj
# Build and publish a release
RUN dotnet publish PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Worker/PlantBasedPizza.Orders.Worker.csproj -c Release -o out -r linux-arm64

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "PlantBasedPizza.Orders.Worker.dll"]