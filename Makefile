build:
	docker build -f src/PlantBasedPizza.Api/application/PlantBasedPizza.Api/Dockerfile -t plant-based-pizza-monolith ./src

tag-images:
	docker tag plant-based-pizza-monolith plantpowerjames/plant-based-pizza-monolith:${IMAGE_TAG}

push:
	docker push plantpowerjames/plant-based-pizza-monolith:${IMAGE_TAG}

unit-test:
	dotnet test src/PlantBasedPizza.Api/tests/PlantBasedPizza.UnitTest