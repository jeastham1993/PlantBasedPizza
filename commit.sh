docker build -f src/PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Worker/Dockerfile -t plant-based-orders-worker-mod-5 ./src |
docker build -f src/PlantBasedPizza.Orders/application/PlantBasedPizza.Orders.Api/Dockerfile -t plant-based-orders-api-mod-5 ./src |

docker build -f src/PlantBasedPizza.Kitchen/application/PlantBasedPizza.Kitchen.Worker/Dockerfile -t plant-based-kitchen-worker-mod-5 ./src |
docker build -f src/PlantBasedPizza.Kitchen/application/PlantBasedPizza.Kitchen.Api/Dockerfile -t plant-based-kitchen-api-mod-5 ./src |

docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Api/Dockerfile -t loyalty-api-mod-5 ./src |
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Internal/Dockerfile -t loyalty-internal-api-mod-5 ./src |
docker build -f src/PlantBasedPizza.LoyaltyPoints/application/PlantBasedPizza.LoyaltyPoints.Worker/Dockerfile -t loyalty-worker-mod-5 ./src |

docker build -f src/PlantBasedPizza.Payments/application/PlantBasedPizza.Payments/Dockerfile -t payment-api-mod-5 ./src |

docker build -f src/PlantBasedPizza.Recipes/applications/PlantBasedPizza.Recipes.Api/Dockerfile -t plant-based-recipes-api-mod-5 ./src |

docker build -f src/PlantBasedPizza.Delivery/application/PlantBasedPizza.Delivery.Worker/Dockerfile -t plant-based-delivery-worker-mod-5 ./src |
docker build -f src/PlantBasedPizza.Delivery/application/PlantBasedPizza.Delivery.Api/Dockerfile -t plant-based-delivery-api-mod-5 ./src|

docker build -f src/PlantBasedPizza.Account/application/PlantBasedPizza.Account.Api/Dockerfile -t plant-based-account-api-mod-5 ./src