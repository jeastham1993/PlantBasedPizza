dotnet clean

dotnet restore src/application/PlantBasedPizza.Api/PlantBasedPizza.Api.csproj

dotnet build src/application/PlantBasedPizza.Api/PlantBasedPizza.Api.csproj

dotnet test tests/PlantBasedPizza.UnitTest/PlantBasedPizza.UnitTest.csproj

docker build -f src/PlantBasedPizza.Api/application/PlantBasedPizza.Api/Dockerfile -t plant-based-pizza-api ./src
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/Dockerfile -t loyalty-api ./src