FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Internal/PlantBasedPizza.Api.Internal/PlantBasedPizza.Api.Internal.csproj", "src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Internal/PlantBasedPizza.Api.Internal/"]
RUN dotnet restore "src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Internal/PlantBasedPizza.Api.Internal/PlantBasedPizza.Api.Internal.csproj"
COPY . .
WORKDIR "/src/src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Internal/PlantBasedPizza.Api.Internal"
RUN dotnet build "PlantBasedPizza.Api.Internal.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PlantBasedPizza.Api.Internal.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlantBasedPizza.Api.Internal.dll"]
