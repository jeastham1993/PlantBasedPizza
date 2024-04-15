dotnet clean

dotnet restore src/application/PlantBasedPizza.Api/PlantBasedPizza.Api.csproj

dotnet build src/application/PlantBasedPizza.Api/PlantBasedPizza.Api.csproj

dotnet test tests/PlantBasedPizza.UnitTest/PlantBasedPizza.UnitTest.csproj

docker build -f src/PlantBasedPizza.Api/application/PlantBasedPizza.Api/Dockerfile -t plant-based-pizza-api ./src |
docker build -f src/PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Worker/Dockerfile -t plant-based-orders-worker ./src |
docker build -f src/PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Api/Dockerfile -t plant-based-orders-api ./src |
docker build -f src/PlantBasedPizza.Kitchen/application/PlantBasedPizza.Kitchen.Worker/Dockerfile -t plant-based-kitchen-worker ./src |
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Api/Dockerfile -t loyalty-api ./src |
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Internal/Dockerfile -t loyalty-internal-api ./src |
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Worker/Dockerfile -t loyalty-worker ./src |
docker build -f src/PlantBasedPizza.Payments/application/PlantBasedPizza.Payments/Dockerfile -t payment-api ./src