build:
	docker build -f application/PlantBasedPizza.LoyaltyPoints.Api/Dockerfile-x86 -t loyalty-api ../
	docker build -f application/PlantBasedPizza.LoyaltyPoints.Internal/Dockerfile-x86 -t loyalty-internal-api ../
	docker build -f application/PlantBasedPizza.LoyaltyPoints.Worker/Dockerfile-x86 -t loyalty-worker ../

build-arm:
	docker build -f application/PlantBasedPizza.LoyaltyPoints.Api/Dockerfile -t loyalty-api ../
	docker build -f application/PlantBasedPizza.LoyaltyPoints.Internal/Dockerfile -t loyalty-internal-api ../
	docker build -f application/PlantBasedPizza.LoyaltyPoints.Worker/Dockerfile -t loyalty-worker ../

tag-images:
	docker tag loyalty-api plantpowerjames/plant-based-pizza-loyalty-api:${IMAGE_TAG}
	docker tag loyalty-internal-api plantpowerjames/plant-based-pizza-loyalty-internal-api:${IMAGE_TAG}
	docker tag loyalty-worker plantpowerjames/plant-based-pizza-loyalty-worker:${IMAGE_TAG}

push:
	docker push plantpowerjames/plant-based-pizza-loyalty-api:${IMAGE_TAG}
	docker push plantpowerjames/plant-based-pizza-loyalty-internal-api:${IMAGE_TAG}
	docker push plantpowerjames/plant-based-pizza-loyalty-worker:${IMAGE_TAG}

unit-test:
	dotnet test tests/PlantBasedPizza.LoyaltyPoints.UnitTest

integration-test:
	docker compose -f docker-compose-integration.yml up -d
	sleep 5
	dotnet test tests/PlantBasedPizza.LoyaltyPoints.IntegrationTest
	docker compose -f docker-compose-integration.yml down

dapr-api-sidecar:
	dapr run --app-id order-api --dapr-http-port 3513 --dapr-grpc-port 40002 --app-port 5271 --resources-path ./components/

local-api:
	export DAPR_GRPC_PORT=40002;export DAPR_HTTP_PORT=3513;dotnet run --project ./application/PlantBasedPizza.LoyaltyPoints.Api/PlantBasedPizza.LoyaltyPoints.Api.csproj

dapr-worker-sidecar:
	dapr run --app-id order-worker --dapr-http-port 3514 --dapr-grpc-port 40003 --app-port 5072 --resources-path ./components/

local-worker:
	export DAPR_GRPC_PORT=40003;export DAPR_HTTP_PORT=3514;dotnet run --project application/PlantBasedPizza.LoyaltyPoints.Worker/PlantBasedPizza.LoyaltyPoints.Worker.csproj