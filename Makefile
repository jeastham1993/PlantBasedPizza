build:
	docker build -f src/PlantBasedPizza.Api/application/PlantBasedPizza.Api/Dockerfile -t plant-based-pizza-monolith ./src

build-loyalty:
	docker build -f src/PlantBasedPizza.LoyaltyPoints/application/Dockerfile -t plant-based-pizza-loyalty-service ./src

tag-images:
	docker tag plant-based-pizza-monolith plantpowerjames/plant-based-pizza-monolith:${IMAGE_TAG}
	docker tag plant-based-pizza-loyalty-service plantpowerjames/plant-based-pizza-monolith-loyalty:${IMAGE_TAG}

push:
	docker push plantpowerjames/plant-based-pizza-monolith:${IMAGE_TAG}
	docker push plantpowerjames/plant-based-pizza-monolith-loyalty:${IMAGE_TAG}

unit-test:
	dotnet test src/PlantBasedPizza.Api/tests/PlantBasedPizza.UnitTest