build: build-api build-worker

build-api:
	docker build -f application/PlantBasedPizza.Kitchen.Api/Dockerfile-x86 -t kitchen-api ../

build-worker:
	docker build -f application/PlantBasedPizza.Kitchen.Worker/Dockerfile-x86 -t kitchen-worker ../

build-arm: build-api-arm build-worker-arm

build-api-arm:
	docker build -f application/PlantBasedPizza.Kitchen.Api/Dockerfile -t kitchen-api ../

build-worker-arm:
	docker build -f application/PlantBasedPizza.Kitchen.Worker/Dockerfile -t kitchen-worker ../

tag-images:
	docker tag kitchen-api plantpowerjames/plant-based-pizza-kitchen-api:${IMAGE_TAG}
	docker tag kitchen-worker plantpowerjames/plant-based-pizza-kitchen-worker:${IMAGE_TAG}

push:
	docker push plantpowerjames/plant-based-pizza-kitchen-api:${IMAGE_TAG}
	docker push plantpowerjames/plant-based-pizza-kitchen-worker:${IMAGE_TAG}

unit-test:
	dotnet test tests/PlantBasedPizza.Kitchen.UnitTests

integration-test: start-test-infra run-integration-tests destory-test-infra

start-test-infra:
	docker compose -f docker-compose.yml -f docker-compose-integration.yml up -d

destory-test-infra:
	docker compose -f docker-compose-integration.yml down

run-integration-tests:
	sleep 5
	dotnet test tests/PlantBasedPizza.Kitchen.IntegrationTests

dapr-api-sidecar:
	dapr run --app-id order-api --dapr-http-port 3513 --dapr-grpc-port 40002 --app-port 5097 --resources-path ./components/

local-api:
	export DAPR_GRPC_PORT=40002;export DAPR_HTTP_PORT=3513;dotnet run --project ./application/PlantBasedPizza.Kitchen.Api/PlantBasedPizza.Kitchen.Api.csproj

dapr-worker-sidecar:
	dapr run --app-id order-worker --dapr-http-port 3514 --dapr-grpc-port 40003 --app-port 5090 --resources-path ./components/

local-worker:
	export DAPR_GRPC_PORT=40003;export DAPR_HTTP_PORT=3514;dotnet run --project application/PlantBasedPizza.Kitchen.Worker/PlantBasedPizza.Kitchen.Worker.csproj