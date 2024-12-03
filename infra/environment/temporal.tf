resource "azurerm_user_assigned_identity" "temporal_app_identity" {
  location            = azurerm_resource_group.plant_based_pizza_rg.location
  name                = "temporalAppIdentity-${var.env}"
  resource_group_name = azurerm_resource_group.plant_based_pizza_rg.name
}

resource "azurerm_container_app" "temporal-server" {
  name                         = "temporal"
  container_app_environment_id = azurerm_container_app_environment.plant_based_pizza_aca_environment.id
  resource_group_name          = azurerm_resource_group.plant_based_pizza_rg.name
  revision_mode                = "Single"
  identity {
    identity_ids = [azurerm_user_assigned_identity.temporal_app_identity.id]
    type         = "UserAssigned"
  }
  ingress {
    external_enabled = true
    target_port      = 7233
    transport = "http2"
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
  template {
    min_replicas = 1
    max_replicas = 1
    container {
      name   = "temporal"
      image  = "temporalio/auto-setup:1.25.2"
      cpu    = 0.25
      memory = "0.5Gi"
      env {
        name  = "DB"
        value = "postgres12"
      }
      env {
        name  = "DB_PORT"
        value = "5432"
      }
      env {
        name = "POSTGRES_PWD"
        value = var.temporal_db_password
      }
      env {
        name = "POSTGRES_USER"
        value = var.temporal_db_user_name
      }
      env {
        name  = "POSTGRES_SEEDS"
        value = "temporal-postgres"
      }
      env {
        name  = "DBNAME"
        value = "postgres"
      }
    }
  }
}

resource "azurerm_container_app" "temporal-ui" {
  name                         = "temporal-ui"
  container_app_environment_id = azurerm_container_app_environment.plant_based_pizza_aca_environment.id
  resource_group_name          = azurerm_resource_group.plant_based_pizza_rg.name
  revision_mode                = "Single"
  identity {
    identity_ids = [azurerm_user_assigned_identity.temporal_app_identity.id]
    type         = "UserAssigned"
  }
  ingress {
    external_enabled = true
    target_port      = 8080
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
  template {
    min_replicas = 1
    max_replicas = 1
    container {
      name   = "temporal"
      image  = "temporalio/ui:2.23.0"
      cpu    = 0.25
      memory = "0.5Gi"
      env {
        name  = "TEMPORAL_ADDRESS"
        value = "${azurerm_container_app.temporal-server.ingress[0].fqdn}:443"
      }
    }
  }
}
