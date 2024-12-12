resource "azurerm_resource_group" "plant_based_pizza_rg" {
  name     = "plant-based-pizza-${var.env}"
  location = "West Europe"
  tags = {
    source = "terraform"
    env = var.env
  }
}

resource "azurerm_log_analytics_workspace" "plant_based_pizza_log_analytics" {
  name                = "plant-based-pizza-logs-${var.env}"
  location            = azurerm_resource_group.plant_based_pizza_rg.location
  resource_group_name = azurerm_resource_group.plant_based_pizza_rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags = {
    source = "terraform"
    env = var.env
  }
}

resource "azurerm_container_app_environment" "plant_based_pizza_aca_environment" {
  name                       = var.env
  location                   = azurerm_resource_group.plant_based_pizza_rg.location
  resource_group_name        = azurerm_resource_group.plant_based_pizza_rg.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.plant_based_pizza_log_analytics.id
  
  tags = {
    source = "terraform"
    env = var.env
  }
}