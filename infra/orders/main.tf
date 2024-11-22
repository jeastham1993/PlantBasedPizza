resource "azurerm_container_app" "orders-api" {
  name                         = "orders"
  container_app_environment_id = data.azurerm_container_app_environment.env.id
  resource_group_name          = data.azurerm_resource_group.plant_based_pizza_rg.name
  revision_mode                = "Single"
  dapr {
    app_id = "orders"
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
      image  = "plantpowerjames/plant-based-pizza-order-api:${var.app_version}"
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
        name = "Services__Loyalty"
        value = "loyalty"
      }
      env {
        name = "Services__LoyaltyInternal"
        value = "http://localhost:50001"
      }
      env {
        name = "Services__PaymentInternal"
        value = "http://localhost:50001"
      }
      env {
        name = "Services__Recipes"
        value = "recipes"
      }
      env {
        name = "Messaging__UseAsyncApi"
        value = "Y"
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
        name = "Auth__PaymentApiKey"
        value = "the api key to use, use a secret store in production"
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
        value = "orders"
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

resource "azurerm_container_app" "orders-worker" {
  name                         = "orders-worker"
  container_app_environment_id = data.azurerm_container_app_environment.env.id
  resource_group_name          = data.azurerm_resource_group.plant_based_pizza_rg.name
  revision_mode                = "Single"
  dapr {
    app_id = "orders-worker"
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
      image  = "plantpowerjames/plant-based-pizza-order-worker:${var.app_version}"
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
        name = "Services__Loyalty"
        value = "loyalty"
      }
      env {
        name = "Services__LoyaltyInternal"
        value = "http://localhost:50001"
      }
      env {
        name = "Services__PaymentInternal"
        value = "http://localhost:50001"
      }
      env {
        name = "Services__Recipes"
        value = "recipes"
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
        value = "orders"
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