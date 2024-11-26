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

integration-test:
	docker compose -f docker-compose-integration.yml up -d
	sleep 5
	dotnet test tests/PlantBasedPizza.Kitchen.IntegrationTests
	docker compose -f docker-compose-integration.yml down