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