build: build-api build-worker build-internal
	
build-worker:
	docker build -f application/PlantBasedPizza.Orders.Worker/Dockerfile-x86 -t order-worker ../
build-api:
	docker build -f application/PlantBasedPizza.Orders.Api/Dockerfile-x86 -t order-api ../
build-internal:
	docker build -f application/PlantBasedPizza.Orders.Internal/Dockerfile-x86 -t order-internal ../

build-arm: build-api-arm build-worker-arm build-internal-arm
	
build-worker-arm:
	docker build -f application/PlantBasedPizza.Orders.Worker/Dockerfile -t order-worker ../
build-api-arm:
	docker build -f application/PlantBasedPizza.Orders.Api/Dockerfile -t order-api ../
build-internal-arm:
	docker build -f application/PlantBasedPizza.Orders.Internal/Dockerfile -t order-internal ../

tag-images:
	docker tag order-api plantpowerjames/plant-based-pizza-order-api:${IMAGE_TAG}
	docker tag order-worker plantpowerjames/plant-based-pizza-order-worker:${IMAGE_TAG}
	docker tag order-internal plantpowerjames/plant-based-pizza-order-internal:${IMAGE_TAG}

push:
	docker push plantpowerjames/plant-based-pizza-order-api:${IMAGE_TAG}
	docker push plantpowerjames/plant-based-pizza-order-worker:${IMAGE_TAG}
	docker push plantpowerjames/plant-based-pizza-order-internal:${IMAGE_TAG}

unit-test:
	dotnet test tests/PlantBasedPizza.Orders.UnitTest

integration-test-infra:
	docker compose -f docker-compose.yml -f docker-compose-integration.yml up -d

integration-test: start-integration-test
	sleep 5
	dotnet test tests/PlantBasedPizza.Orders.IntegrationTest
	docker compose -f docker-compose.yml -f docker-compose-integration.yml down

dapr-api-sidecar:
	dapr run --app-id order-api --dapr-http-port 3513 --dapr-grpc-port 40002 --app-port 5004 --resources-path ./components/

local-api:
	export DAPR_GRPC_PORT=40002;export DAPR_HTTP_PORT=3513;dotnet run --project ./application/PlantBasedPizza.Orders.Api/PlantBasedPizza.Orders.Api.csproj

dapr-worker-sidecar:
	dapr run --app-id order-worker --dapr-http-port 3514 --dapr-grpc-port 40003 --app-port 5074 --resources-path ./components/

local-worker:
	export DAPR_GRPC_PORT=40003;export DAPR_HTTP_PORT=3514;dotnet run --project application/PlantBasedPizza.Orders.Worker/PlantBasedPizza.Orders.Worker.csproj