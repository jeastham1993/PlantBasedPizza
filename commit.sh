docker build -f src/PlantBasedPizza.Api/application/PlantBasedPizza.Api/Dockerfile -t plant-based-pizza-api-mod-35 ./src |
docker build -f src/PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Worker/Dockerfile -t plant-based-orders-worker-mod-35 ./src |
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Api/Dockerfile -t loyalty-api-mod-35 ./src |
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Internal/Dockerfile -t loyalty-internal-api-mod-35 ./src |
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Worker/Dockerfile -t loyalty-worker-mod-35 ./src |
docker build -f src/PlantBasedPizza.Payments/application/PlantBasedPizza.Payments/Dockerfile -t payment-api-mod-35 ./src