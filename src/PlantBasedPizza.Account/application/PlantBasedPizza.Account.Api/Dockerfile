FROM mcr.microsoft.com/dotnet/sdk:9.0 as build

WORKDIR /App
COPY . ./
RUN dotnet restore PlantBasedPizza.Account/application/PlantBasedPizza.Account.Api/PlantBasedPizza.Account.Api.csproj
RUN dotnet publish PlantBasedPizza.Account/application/PlantBasedPizza.Account.Api/PlantBasedPizza.Account.Api.csproj -o out -c Release -r linux-arm64

FROM mcr.microsoft.com/dotnet/aspnet:9.0 as runtime
WORKDIR /App
COPY --from=build /App/out .
EXPOSE 80
ENTRYPOINT [ "dotnet", "PlantBasedPizza.Account.Api.dll" ]