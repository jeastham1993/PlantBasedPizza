FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build
WORKDIR /app
COPY . .
RUN dotnet restore application/PlantBasedPizza.Api/PlantBasedPizza.Api.csproj
RUN dotnet publish application/PlantBasedPizza.Api/PlantBasedPizza.Api.csproj -o /app/published-app -c Release -r linux-x64

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine as runtime
WORKDIR /app
COPY --from=build /app/published-app /app
EXPOSE 80
ENTRYPOINT [ "dotnet", "/app/PlantBasedPizza.Api.dll" ]