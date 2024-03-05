FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

WORKDIR /App
COPY . ./
RUN dotnet restore application/PlantBasedPizza.Api/PlantBasedPizza.Api.csproj
RUN dotnet publish application/PlantBasedPizza.Api/PlantBasedPizza.Api.csproj -o out -c Release

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
WORKDIR /App
COPY --from=build /App/out .
EXPOSE 80
ENTRYPOINT [ "dotnet", "PlantBasedPizza.Api.dll" ]