resource "azurerm_storage_account" "functions_storage_account" {
  name                     = "pbprecipesstorage"
  resource_group_name      = data.azurerm_resource_group.plant_based_pizza_rg.name
  location                 = data.azurerm_resource_group.plant_based_pizza_rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_service_plan" "functions_app_service_plan" {
  name                = "plantbasedpizza-recipes-app-service-plan"
  resource_group_name = data.azurerm_resource_group.plant_based_pizza_rg.name
  location            = data.azurerm_resource_group.plant_based_pizza_rg.location
  os_type             = "Linux"
  sku_name            = "Y1"
}

resource "azurerm_linux_function_app" "function_app" {
  name                = "plantbasedpizza-recipes-function-app"
  resource_group_name = data.azurerm_resource_group.plant_based_pizza_rg.name
  location            = data.azurerm_resource_group.plant_based_pizza_rg.location

  storage_account_name       = azurerm_storage_account.functions_storage_account.name
  storage_account_access_key = azurerm_storage_account.functions_storage_account.primary_access_key
  service_plan_id            = azurerm_service_plan.functions_app_service_plan.id


  site_config {}
  app_settings = {
    "DatabaseConnection"                 = var.db_connection_string
    "Environment"                        = var.env
    "DOMAIN"                             = "recipes"
    "ApplicationConfig__TeamName"        = "recipes"
    "ApplicationConfig__ApplicationName" = "recipes-api"
    "ApplicationConfig__Environment"     = var.env
    "ApplicationConfig__Version"         = var.app_version
    "ApplicationConfig__DeployedAt"      = var.app_version
    "ApplicationConfig__MemoryMb"        = "500"
    "ApplicationConfig__CpuCount"        = "0.25"
    "ApplicationConfig__CloudRegion"     = "europe-west2"
    "Auth__Issuer"                       = "https://plantbasedpizza.com"
    "Auth__Audience"                     = "https://plantbasedpizza.com"
    "Auth__Key"                          = "This is a sample secret key - please don't use in production environment."
    "MOMENTO_API_KEY"                    = var.momento_api_key
    "CACHE_NAME"                         = var.cache_name
  }
}
