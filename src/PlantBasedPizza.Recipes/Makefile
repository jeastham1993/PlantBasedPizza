build:
	docker build -f applications/PlantBasedPizza.Recipes.Api/Dockerfile-x86 -t recipe-api ../

build-arm:
	docker build -f applications/PlantBasedPizza.Recipes.Api/Dockerfile -t recipe-api ../

tag-images:
	docker tag recipe-api plantpowerjames/plant-based-pizza-recipe-api:${IMAGE_TAG}

push:
	docker push plantpowerjames/plant-based-pizza-recipe-api:${IMAGE_TAG}

unit-test:
	dotnet test tests/PlantBasedPizza.Recipes.UnitTests

integration-test:
	docker compose -f docker-compose-integration.yml up -d
	sleep 5
	dotnet test tests/PlantBasedPizza.Recipes.IntegrationTests
	docker compose -f docker-compose-integration.yml down

dapr-api-sidecar:
	dapr run --app-id order-api --dapr-http-port 3513 --dapr-grpc-port 40002 --app-port 5138 --resources-path ./components/

local-api:
	export DAPR_GRPC_PORT=40002;export DAPR_HTTP_PORT=3513;dotnet run --project ./application/PlantBasedPizza.Recipes.Api/PlantBasedPizza.Recipes.Api.csproj