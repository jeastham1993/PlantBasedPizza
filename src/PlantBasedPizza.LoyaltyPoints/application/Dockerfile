FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.csproj
# Build and publish a release
RUN dotnet publish PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "PlantBasedPizza.LoyaltyPoints.dll"]