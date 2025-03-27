data "azurerm_servicebus_namespace" "example" {
  name                = var.public_service_bus_namespace
  resource_group_name = var.resource_group_name
}