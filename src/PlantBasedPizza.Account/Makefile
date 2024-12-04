build:
	docker build -f application/PlantBasedPizza.Account.Api/Dockerfile-x86 -t account-api ../

build-arm:
	docker build -f application/PlantBasedPizza.Account.Api/Dockerfile -t account-api ../

tag-images:
	docker tag account-api plantpowerjames/plant-based-pizza-account-api:${IMAGE_TAG}

push:
	docker push plantpowerjames/plant-based-pizza-account-api:${IMAGE_TAG}

unit-test:
	dotnet test tests/PlantBasedPizza.Account.UnitTests

integration-test:
	docker compose -f docker-compose-integration.yml up -d
	sleep 5
	dotnet test tests/PlantBasedPizza.Account.IntegrationTests
	docker compose -f docker-compose-integration.yml down

dapr-api-sidecar:
	dapr run --app-id account-api --dapr-http-port 3513 --dapr-grpc-port 40002 --app-port 5139 --resources-path ./components/

local-api:
	export DAPR_GRPC_PORT=40002;export DAPR_HTTP_PORT=3513;dotnet run --project ./application/PlantBasedPizza.Account.Api/PlantBasedPizza.Account.Api.csproj