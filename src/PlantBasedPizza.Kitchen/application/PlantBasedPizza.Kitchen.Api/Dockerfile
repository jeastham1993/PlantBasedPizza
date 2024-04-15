FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

WORKDIR /App
COPY . ./
RUN dotnet restore PlantBasedPizza.Kitchen/application/PlantBasedPizza.Kitchen.Api/PlantBasedPizza.Kitchen.Api.csproj
RUN dotnet publish PlantBasedPizza.Kitchen/application/PlantBasedPizza.Kitchen.Api/PlantBasedPizza.Kitchen.Api.csproj -o out -c Release -r linux-arm64

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
WORKDIR /App
COPY --from=build /App/out .
EXPOSE 80
ENTRYPOINT [ "dotnet", "PlantBasedPizza.Kitchen.Api.dll" ]