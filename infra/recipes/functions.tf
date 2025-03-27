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


resource "azurerm_container_app" "recipes-functions-aca" {
  name                         = "recipes-functions-aca"
  container_app_environment_id = data.azurerm_container_app_environment.env.id
  resource_group_name          = data.azurerm_resource_group.plant_based_pizza_rg.name
  revision_mode                = "Single"
  dapr {
    app_id = "recipes-aca"
    app_port = 8080
    app_protocol = "http"
  }
  identity {
    identity_ids = [ azurerm_user_assigned_identity.app_identity.id ]
    type = "UserAssigned"
  }
  ingress {
    external_enabled = false
    target_port = 8080
    traffic_weight {
      percentage = 100
      latest_revision = true
    }
  }
  template {
    min_replicas = 1
    max_replicas = 1
    container {
      name   = "application"
      image  = "plantpowerjames/plant-based-pizza-recipe-functions:${var.app_version}"
      cpu    = 0.25
      memory = "0.5Gi"
      env {
        name = "DatabaseConnection"
        value = var.db_connection_string
      }
      env {
        name = "Environment"
        value = var.env
      }
      env {
        name = "DOMAIN"
        value = "recipes"
      }
      env {
        name = "ApplicationConfig__TeamName"
        value = "recipes"
      }
      env {
        name = "ApplicationConfig__ApplicationName"
        value = "recipes-api"
      }
      env {
        name = "ApplicationConfig__Environment"
        value = var.env
      }
      env {
        name = "ApplicationConfig__Version"
        value = var.app_version
      }
      env {
        name = "ApplicationConfig__DeployedAt"
        value = var.app_version
      }
      env {
        name = "ApplicationConfig__MemoryMb"
        value = "500"
      }
      env {
        name = "ApplicationConfig__CpuCount"
        value = "0.25"
      }
      env {
        name = "ApplicationConfig__CloudRegion"
        value = "europe-west2"
      }
      env {
        name = "Auth__Issuer"
        value = "https://plantbasedpizza.com"
      }
      env {
        name = "Auth__Audience"
        value = "https://plantbasedpizza.com"
      }
      env {
        name = "Auth__Key"
        value = "This is a sample secret key - please don't use in production environment."
      }
      env {
        name  = "OTEL_EXPORTER_OTLP_ENDPOINT"
        value = "http://localhost:4317"
      }
      env {
        name = "MOMENTO_API_KEY"
        value = var.momento_api_key
      }
      env {
        name = "CACHE_NAME"
        value = var.cache_name
      }
    }
    container {
      name   = "datadog"
      image  = "index.docker.io/datadog/serverless-init:latest"
      cpu    = 0.25
      memory = "0.5Gi"
      env {
        name = "DD_SITE"
        value = var.dd_site
      }
      env {
        name = "DD_API_KEY"
        value = var.dd_api_key
      }
      env {
        name = "DD_ENV"
        value = var.env
      }
      env {
        name = "DD_VERSION"
        value = var.app_version
      }
      env {
        name = "DD_SERVICE"
        value = "recipes"
      }
      env {
        name = "DD_LOGS_ENABLED"
        value = "true"
      }
      env {
        name = "DD_LOGS_INJECTION"
        value = "true"
      }
      env {
        name = "DD_APM_IGNORE_RESOURCES"
        value = "/opentelemetry.proto.collector.trace.v1.TraceService/Export$"
      }
      env {
        name = "DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT"
        value = "0.0.0.0:4317"
      }
      env {
        name = "DD_AZURE_SUBSCRIPTION_ID"
        value = data.azurerm_subscription.primary.subscription_id
      }
      env {
        name = "DD_AZURE_RESOURCE_GROUP"
        value = data.azurerm_resource_group.plant_based_pizza_rg.name
      }
    }
  }
}