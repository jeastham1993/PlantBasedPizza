resource "azurerm_user_assigned_identity" "public_service_bus_identity" {
  location            = azurerm_resource_group.plant_based_pizza_rg.location
  name                = "publicPubSubIdentity-${var.env}"
  resource_group_name = azurerm_resource_group.plant_based_pizza_rg.name
}

resource "azurerm_role_assignment" "azure_service_bus_sender" {
  scope                = azurerm_servicebus_namespace.plant_based_pizza_public_service_bus.id
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = azurerm_user_assigned_identity.public_service_bus_identity.principal_id
}

resource "azurerm_role_assignment" "azure_service_bus_receiver" {
  scope                = azurerm_servicebus_namespace.plant_based_pizza_public_service_bus.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = azurerm_user_assigned_identity.public_service_bus_identity.principal_id
}
