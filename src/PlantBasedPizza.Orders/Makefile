build: build-api build-worker
	
build-worker:
	docker build -f application/PlantBasedPizza.Orders.Worker/Dockerfile -t order-worker ../
build-api:
	docker build -f application/PlantBasedPizza.Orders.Api/Dockerfile -t order-api ../

tag-images:
	docker tag order-api plantpowerjames/plant-based-pizza-order-api:${IMAGE_TAG}
	docker tag order-worker plantpowerjames/plant-based-pizza-order-worker:${IMAGE_TAG}

push:
	docker push plantpowerjames/plant-based-pizza-order-api:${IMAGE_TAG}
	docker push plantpowerjames/plant-based-pizza-order-worker:${IMAGE_TAG}

unit-test:
	dotnet test tests/PlantBasedPizza.Orders.UnitTest

integration-test:
	docker compose -f docker-compose.yml -f docker-compose-integration.yml up -d
	sleep 5
	dotnet test tests/PlantBasedPizza.Orders.IntegrationTest
	docker compose -f docker-compose.yml -f docker-compose-integration.yml down