build:
	docker build -f application/PlantBasedPizza.Delivery.Api/Dockerfile-x86 -t delivery-api ../
	docker build -f application/PlantBasedPizza.Delivery.Worker/Dockerfile-x86 -t delivery-worker ../

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