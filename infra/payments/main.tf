resource "azurerm_container_app" "payment-api" {
  name                         = "payments"
  container_app_environment_id = data.azurerm_container_app_environment.env.id
  resource_group_name          = data.azurerm_resource_group.plant_based_pizza_rg.name
  revision_mode                = "Single"
  dapr {
    app_id = "payment"
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
      image  = "plantpowerjames/plant-based-pizza-payment-api:${var.app_version}"
      cpu    = 0.25
      memory = "0.5Gi"
      env {
        name = "Environment"
        value = var.env
      }
      env {
        name = "DOMAIN"
        value = "payments"
      }
      env {
        name = "ApplicationConfig__TeamName"
        value = "payments"
      }
      env {
        name = "ApplicationConfig__ApplicationName"
        value = "payments-api"
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
        name = "MOMENTO_API_KEY"
        value = var.momento_api_key
      }
      env {
        name = "CACHE_NAME"
        value = var.cache_name
      }
      env {
        name = "Services__OrdersInternal"
        value = "http://localhost:50001"
      }
      env {
        name  = "OTEL_EXPORTER_OTLP_ENDPOINT"
        value = "http://localhost:4317"
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
        value = "payments"
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

resource "azurerm_container_app" "payment-test-harness" {
  name                         = "payment-test-harness"
  container_app_environment_id = data.azurerm_container_app_environment.env.id
  resource_group_name          = data.azurerm_resource_group.plant_based_pizza_rg.name
  revision_mode                = "Single"
  count = var.env == "payments-test" ? 1 : 0
  dapr {
    app_id = "payment-test-harness"
    app_port = 8080
    app_protocol = "http"
  }
  identity {
    identity_ids = [ azurerm_user_assigned_identity.app_identity.id ]
    type = "UserAssigned"
  }
  ingress {
    external_enabled = true
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
      image  = "plantpowerjames/plant-based-pizza-payment-event-harness:${var.app_version}"
      cpu    = 0.25
      memory = "0.5Gi"
      env {
        name = "Environment"
        value = var.env
      }
      env {
        name = "DOMAIN"
        value = "payments"
      }
      env {
        name = "ApplicationConfig__TeamName"
        value = "payments"
      }
      env {
        name = "ApplicationConfig__ApplicationName"
        value = "payments-api"
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
        name = "MOMENTO_API_KEY"
        value = var.momento_api_key
      }
      env {
        name = "CACHE_NAME"
        value = var.cache_name
      }
      env {
        name  = "OTEL_EXPORTER_OTLP_ENDPOINT"
        value = "http://localhost:4317"
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
        value = "payments"
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