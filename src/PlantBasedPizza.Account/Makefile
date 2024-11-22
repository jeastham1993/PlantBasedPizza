build:
	docker build -f application/PlantBasedPizza.Account.Api/Dockerfile-x86 -t account-api ../

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