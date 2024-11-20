orders-deps:
	cd src/PlantBasedPizza.Orders;docker compose up -d

orders-deps-down:
	cd src/PlantBasedPizza.Orders;docker compose down

dapr-orders:
	dapr run --app-id orders --app-port 8080 --components-path ./components -- dotnet run -p src/PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Api --urls http://localhost:8080