build:
	docker build -f application/PlantBasedPizza.Payments/Dockerfile-x86 -t payment-api ../

build-arm:
	docker build -f application/PlantBasedPizza.Payments/Dockerfile -t payment-api ../

tag-images:
	docker tag payment-api plantpowerjames/plant-based-pizza-payment-api:${IMAGE_TAG}

push:
	docker push plantpowerjames/plant-based-pizza-payment-api:${IMAGE_TAG}

unit-test:
	dotnet test tests/PlantBasedPizza.Payments.UnitTests

integration-test:
	docker compose -f docker-compose.yml -f docker-compose-integration.yml up -d
	sleep 5
	dotnet test tests/PlantBasedPizza.Payments.IntegrationTests
	docker compose -f docker-compose.yml -f docker-compose-integration.yml down

dapr-api-sidecar:
	dapr run --app-id payment-api --dapr-http-port 3515 --dapr-grpc-port 40004 --app-port 5070 --resources-path ./components/

local-api:
	export DAPR_GRPC_PORT=40004;export DAPR_HTTP_PORT=3515;dotnet run --project ./application/PlantBasedPizza.Payments/PlantBasedPizza.Payments.csproj