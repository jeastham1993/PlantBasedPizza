build:
	docker build -f application/PlantBasedPizza.Payments/Dockerfile-x86 -t payment-api ../

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