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

resource "azurerm_container_app_environment_storage" "nginx_configuration_storage" {
  name                         = "nginx-config"
  container_app_environment_id = azurerm_container_app_environment.plant_based_pizza_aca_environment.id
  account_name                 = azurerm_storage_account.nginx_config.name
  share_name                   = azurerm_storage_share.nginx_config_share.name
  access_key                   = azurerm_storage_account.nginx_config.primary_access_key
  access_mode                  = "ReadOnly"
}