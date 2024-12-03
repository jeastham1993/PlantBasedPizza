orders-deps:
	cd src/PlantBasedPizza.Orders;docker compose up -d

orders-deps-down:
	cd src/PlantBasedPizza.Orders;docker compose down

dapr-orders:
	dapr run --app-id orders --app-port 8080 --components-path ./components -- dotnet run -p src/PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Api --urls http://localhost:8080

build-account:
	cd src/PlantBasedPizza.Account;make build
build-delivery:
	cd src/PlantBasedPizza.Delivery;make build
build-kitchen:
	cd src/PlantBasedPizza.Kitchen;make build
build-loyalty-points:
	cd src/PlantBasedPizza.LoyaltyPoints;make build
build-orders:
	cd src/PlantBasedPizza.Orders;make build
build-payments:
	cd src/PlantBasedPizza.Payments;make build
build-recipes:
	cd src/PlantBasedPizza.Recipes;make build

build: build-account build-delivery build-kitchen build-loyalty-points build-orders build-payments build-recipes

build-account-arm:
	cd src/PlantBasedPizza.Account;make build-arm
build-delivery-arm:
	cd src/PlantBasedPizza.Delivery;make build-arm
build-kitchen-arm:
	cd src/PlantBasedPizza.Kitchen;make build-arm
build-loyalty-points-arm:
	cd src/PlantBasedPizza.LoyaltyPoints;make build-arm
build-orders-arm:
	cd src/PlantBasedPizza.Orders;make build-arm
build-payments-arm:
	cd src/PlantBasedPizza.Payments;make build-arm
build-recipes-arm:
	cd src/PlantBasedPizza.Recipes;make build-arm

build-arm: build-account-arm build-delivery-arm build-kitchen-arm build-loyalty-points-arm build-orders-arm build-payments-arm build-recipes-arm

redeploy-all: deploy-account deploy-delivery deploy-kitchen deploy-loyalty deploy-orders deploy-payments deploy-recipes
	
deploy-account:
	cd infra/account;terraform init;terraform apply --var-file dev.tfvars -auto-approve
	
deploy-delivery:
	cd infra/delivery;terraform init;terraform apply --var-file dev.tfvars -auto-approve

deploy-kitchen:
	cd infra/kitchen;terraform init;terraform apply --var-file dev.tfvars -auto-approve

deploy-loyalty:
	cd infra/loyalty;terraform init;terraform apply --var-file dev.tfvars -auto-approve

deploy-orders:
	cd infra/orders;terraform init;terraform apply --var-file dev.tfvars -auto-approve

deploy-payments:
	cd infra/payments;terraform init;terraform apply --var-file dev.tfvars -auto-approve

deploy-recipes:
	cd infra/recipes;terraform init;terraform apply --var-file dev.tfvars -auto-approve


configure-temporal:
	./temporal-sql-tool --ep ep-shy-resonance-a5b1xfai.us-east-2.aws.neon.tech -p 5432 --db temporal-visibility --plugin postgres12 setup-schema -v 0.0
	./temporal-sql-tool --ep ep-shy-resonance-a5b1xfai.us-east-2.aws.neon.tech -p 5432 --db temporal-visibility --plugin postgres12 update-schema -d ./schema/postgresql/v12/visibility/versioned