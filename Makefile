orders-deps:
	cd src/PlantBasedPizza.Orders;docker compose up -d

orders-deps-down:
	cd src/PlantBasedPizza.Orders;docker compose down

dapr-orders:
	dapr run --app-id orders --app-port 8080 --components-path ./components -- dotnet run -p src/PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Api --urls http://localhost:8080

redeploy-all: deploy-account deploy-delivery deploy-kitchen deploy-loyalty deploy-orders deploy-payments deploy-recipes
	
deploy-account:
	cd infra/account;terraform apply --var-file dev.tfvars -auto-approve
	
deploy delivery:
	cd infra/delivery;terraform apply --var-file dev.tfvars -auto-approve

deploy-kitchen:
	cd infra/kitchen;terraform apply --var-file dev.tfvars -auto-approve

deploy-loyalty:
	cd infra/loyalty;terraform apply --var-file dev.tfvars -auto-approve

deploy-orders:
	cd infra/orders;terraform apply --var-file dev.tfvars -auto-approve

deploy-payments:
	cd infra/payments;terraform apply --var-file dev.tfvars -auto-approve

deploy-recipes:
	cd infra/recipes;terraform apply --var-file dev.tfvars -auto-approve
