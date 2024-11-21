resource "azurerm_user_assigned_identity" "app_identity" {
  location            = data.azurerm_resource_group.plant_based_pizza_rg.location
  name                = "ordersAppIdentity-${var.env}"
  resource_group_name = data.azurerm_resource_group.plant_based_pizza_rg.name
}