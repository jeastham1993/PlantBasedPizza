docker-compose up -d

sleep 3s

dotnet test tests/PlantBasedPizza.IntegrationTests/PlantBasedPizza.IntegrationTests.csproj

docker-compose down -v