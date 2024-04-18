dotnet clean

dotnet restore src/application/PlantBasedPizza.Api/PlantBasedPizza.Api.csproj

dotnet build src/application/PlantBasedPizza.Api/PlantBasedPizza.Api.csproj

dotnet test tests/PlantBasedPizza.UnitTest/PlantBasedPizza.UnitTest.csproj

docker build -f src/PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Worker/Dockerfile -t plant-based-orders-worker ./src |
docker build -f src/PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Api/Dockerfile -t plant-based-orders-api ./src |

docker build -f src/PlantBasedPizza.Kitchen/application/PlantBasedPizza.Kitchen.Worker/Dockerfile -t plant-based-kitchen-worker ./src |
docker build -f src/PlantBasedPizza.Kitchen/application/PlantBasedPizza.Kitchen.Api/Dockerfile -t plant-based-kitchen-api ./src |

docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Api/Dockerfile -t loyalty-api ./src |
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Internal/Dockerfile -t loyalty-internal-api ./src |
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Worker/Dockerfile -t loyalty-worker ./src |

docker build -f src/PlantBasedPizza.Payments/application/PlantBasedPizza.Payments/Dockerfile -t payment-api ./src |

docker build -f src/PlantBasedPizza.Recipes/applications/PlantBasedPizza.Recipes.Api/Dockerfile -t plant-based-recipes-api ./src |

docker build -f src/PlantBasedPizza.Delivery/application/PlantBasedPizza.Delivery.Worker/Dockerfile -t plant-based-delivery-worker ./src |
docker build -f src/PlantBasedPizza.Delivery/application/PlantBasedPizza.Delivery.Api/Dockerfile -t plant-based-delivery-api ./src

docker build -f src/PlantBasedPizza.Account/application/PlantBasedPizza.Account.Api/Dockerfile -t plant-based-account-api ./src