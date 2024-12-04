build: build-api build-worker

build-api:
	docker build -f application/PlantBasedPizza.Delivery.Api/Dockerfile-x86 -t delivery-api ../

build-worker:
	docker build -f application/PlantBasedPizza.Delivery.Worker/Dockerfile-x86 -t delivery-worker ../

build-arm: build-api-arm build-worker-arm

build-api-arm:
	docker build -f application/PlantBasedPizza.Delivery.Api/Dockerfile -t delivery-api ../

build-worker-arm:
	docker build -f application/PlantBasedPizza.Delivery.Worker/Dockerfile -t delivery-worker ../

tag-images:
	docker tag delivery-api plantpowerjames/plant-based-pizza-delivery-api:${IMAGE_TAG}
	docker tag delivery-worker plantpowerjames/plant-based-pizza-delivery-worker:${IMAGE_TAG}

push:
	docker push plantpowerjames/plant-based-pizza-delivery-api:${IMAGE_TAG}
	docker push plantpowerjames/plant-based-pizza-delivery-worker:${IMAGE_TAG}

unit-test:
	dotnet test tests/PlantBasedPizza.Delivery.UnitTests

integration-test:
	docker compose -f docker-compose-integration.yml up -d
	sleep 5
	docker ps
	dotnet test tests/PlantBasedPizza.Delivery.IntegrationTests
	docker compose -f docker-compose-integration.yml down

dapr-api-sidecar:
	dapr run --app-id order-api --dapr-http-port 3513 --dapr-grpc-port 40002 --app-port 5259 --resources-path ./components/

local-api:
	export DAPR_GRPC_PORT=40002;export DAPR_HTTP_PORT=3513;dotnet run --project ./application/PlantBasedPizza.Delivery.Api/PlantBasedPizza.Delivery.Api.csproj

dapr-worker-sidecar:
	dapr run --app-id order-worker --dapr-http-port 3514 --dapr-grpc-port 40003 --app-port 5090 --resources-path ./components/

local-worker:
	export DAPR_GRPC_PORT=40003;export DAPR_HTTP_PORT=3514;dotnet run --project application/PlantBasedPizza.Delivery.Worker/PlantBasedPizza.Delivery.Worker.csproj