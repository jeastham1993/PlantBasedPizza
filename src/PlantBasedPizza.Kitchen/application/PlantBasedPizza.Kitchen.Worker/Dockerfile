FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore PlantBasedPizza.Kitchen/application/PlantBasedPizza.Kitchen.Worker/PlantBasedPizza.Kitchen.Worker.csproj
# Build and publish a release
RUN dotnet publish PlantBasedPizza.Kitchen/application/PlantBasedPizza.Kitchen.Worker/PlantBasedPizza.Kitchen.Worker.csproj -c Release -o out -r linux-arm64

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "PlantBasedPizza.Kitchen.Worker.dll"]