resource "azurerm_servicebus_namespace" "plant_based_pizza_public_service_bus" {
  name                = "plant-based-pizza-public-${var.env}"
  location            = azurerm_resource_group.plant_based_pizza_rg.location
  resource_group_name = azurerm_resource_group.plant_based_pizza_rg.name
  sku                 = "Standard"
  tags = {
    source = "terraform"
    env    = var.env
  }
}

resource "azurerm_container_app_environment_dapr_component" "public_pubsub" {
  name                         = "public"
  container_app_environment_id = azurerm_container_app_environment.plant_based_pizza_aca_environment.id
  component_type               = "pubsub.azure.servicebus.topics"
  version                      = "v1"
  scopes                       = [
    "account",
    "orders",
    "orders-worker",
    "loyalty",
    "loyaltyworker",
    "kitchen",
    "kitchen-worker",
    "delivery",
    "delivery-worker",
    "recipes",
    "payment"
  ]
  metadata {
    name  = "connectionString"
    value = azurerm_servicebus_namespace.plant_based_pizza_public_service_bus.default_primary_connection_string
  }
  metadata {
    name  = "azureClientId"
    value = azurerm_user_assigned_identity.public_service_bus_identity.client_id
  }
}

resource "azurerm_container_app_environment_dapr_component" "payments_pubsub" {
  name                         = "public"
  container_app_environment_id = azurerm_container_app_environment.plant_based_pizza_aca_environment.id
  component_type               = "pubsub.azure.servicebus.queues"
  version                      = "v1"
  scopes                       = [
    "orders",
    "orders-worker",
    "payment"
  ]
  metadata {
    name  = "connectionString"
    value = azurerm_servicebus_namespace.plant_based_pizza_public_service_bus.default_primary_connection_string
  }
  metadata {
    name  = "azureClientId"
    value = azurerm_user_assigned_identity.public_service_bus_identity.client_id
  }
}
