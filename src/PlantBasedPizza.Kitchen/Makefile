build:
	docker build -f application/PlantBasedPizza.Kitchen.Api/Dockerfile -t kitchen-api ../
	docker build -f application/PlantBasedPizza.Kitchen.Worker/Dockerfile -t kitchen-worker ../

tag-images:
	docker tag delivery-api plantpowerjames/plant-based-pizza-kitchen-api:${IMAGE_TAG}
	docker tag delivery-worker plantpowerjames/plant-based-pizza-kitchen-worker:${IMAGE_TAG}

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