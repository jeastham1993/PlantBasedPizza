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

data "azurerm_servicebus_topic" "order_completed_topic" {
  name         = "order.orderCompleted.v2"
  namespace_id = var.public_service_bus_namespace
}

resource "azurerm_servicebus_subscription" "order_completed_topic_subscription" {
  name               = "recipe-service"
  topic_id           = data.azurerm_servicebus_topic.order_completed_topic.id
  max_delivery_count = 1
}

resource "azurerm_linux_function_app" "function_app" {
  name                = "plantbasedpizza-recipes-function-app"
  resource_group_name = data.azurerm_resource_group.plant_based_pizza_rg.name
  location            = data.azurerm_resource_group.plant_based_pizza_rg.location

  storage_account_name       = azurerm_storage_account.functions_storage_account.name
  storage_account_access_key = azurerm_storage_account.functions_storage_account.primary_access_key
  service_plan_id            = azurerm_service_plan.functions_app_service_plan.id
  https_only                 = true

  site_config {
    application_stack {
      use_dotnet_isolated_runtime = true
      dotnet_version = "9.0"
    }
  }
  app_settings = {
    "FUNCTIONS_EXTENSION_VERSION" : "~4"
    "FUNCTIONS_WORKER_RUNTIME" : "DOTNET-ISOLATED"
    "SCM_DO_BUILD_DURING_DEPLOYMENT" : 0,
    "AzureWebJobsStorage" : azurerm_storage_account.functions_storage_account.primary_connection_string
    "AzureWebJobsDashboard" : azurerm_storage_account.functions_storage_account.primary_connection_string
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" : azurerm_storage_account.functions_storage_account.primary_connection_string

    "DatabaseConnection"                  = var.db_connection_string
    "Environment"                         = var.env
    "DOMAIN"                              = "recipes"
    "ApplicationConfig__TeamName"         = "recipes"
    "ApplicationConfig__ApplicationName"  = "recipes-api"
    "ApplicationConfig__Environment"      = var.env
    "ApplicationConfig__Version"          = var.app_version
    "ApplicationConfig__DeployedAt"       = var.app_version
    "ApplicationConfig__MemoryMb"         = "500"
    "ApplicationConfig__CpuCount"         = "0.25"
    "ApplicationConfig__CloudRegion"      = "europe-west2"
    "Auth__Issuer"                        = "https://plantbasedpizza.com"
    "Auth__Audience"                      = "https://plantbasedpizza.com"
    "Auth__Key"                           = "This is a sample secret key - please don't use in production environment."
    "MOMENTO_API_KEY"                     = var.momento_api_key
    "CACHE_NAME"                          = var.cache_name
    "AZURE_SERVICE_BUS_CONNECTION_STRING" = data.azurerm_servicebus_namespace.example.default_primary_connection_string
  }
}