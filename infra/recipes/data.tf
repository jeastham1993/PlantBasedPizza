data "azurerm_subscription" "primary" {
}

data "azurerm_resource_group" "plant_based_pizza_rg" {
  name = var.resource_group_name
}

data "azurerm_container_app_environment" "env" {
  name                = var.env
  resource_group_name = var.resource_group_name
}
data "azurerm_servicebus_namespace" "public_service_bus" {
  name                = var.public_service_bus_namespace
  resource_group_name = var.resource_group_name
}