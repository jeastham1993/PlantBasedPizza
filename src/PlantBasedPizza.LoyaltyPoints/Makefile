build:
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
	dotnet test tests/PlantBasedPizza.LoyaltyPoints.UnitTests

integration-test:
	docker compose -f docker-compose-integration.yml up -d
	sleep 5
	dotnet test tests/PlantBasedPizza.LoyaltyPoints.IntegrationTest
	docker compose -f docker-compose-integration.yml down