resource "azurerm_user_assigned_identity" "app_identity" {
  location            = azurerm_resource_group.plant_based_pizza_rg.location
  name                = "nginxAppIdentity-${var.env}"
  resource_group_name = azurerm_resource_group.plant_based_pizza_rg.name
}

resource "azurerm_storage_account" "nginx_config" {
  name                     = "plantbasedpizzanginx"
  resource_group_name      = azurerm_resource_group.plant_based_pizza_rg.name
  location                 = azurerm_resource_group.plant_based_pizza_rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = {
    environment = var.env
  }
}

resource "azurerm_storage_share" "nginx_config_share" {
  name               = "nginx-config"
  storage_account_name = azurerm_storage_account.nginx_config.name
  quota = 50
}

resource "azurerm_storage_share_file" "nginx_config" {
  name             = "nginx.conf"
  storage_share_id = azurerm_storage_share.nginx_config_share.id
  source           = "${path.root}/nginx.conf"
  content_md5 = filemd5("${path.root}/nginx.conf")
}

resource "azurerm_container_app" "nginx" {
  name                         = "nginx"
  container_app_environment_id = azurerm_container_app_environment.plant_based_pizza_aca_environment.id
  resource_group_name          = azurerm_resource_group.plant_based_pizza_rg.name
  revision_mode                = "Single"
  identity {
    identity_ids = [ azurerm_user_assigned_identity.app_identity.id ]
    type = "UserAssigned"
  }
  ingress {
    external_enabled = true
    target_port = 80
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
      image  = "nginx"
      cpu    = 0.25
      memory = "0.5Gi"
      volume_mounts {
        name = "nginx-config"
        path = "/etc/nginx"
      }
    }
    volume {
      name = "nginx-config"
      storage_name = "nginx-config"
      storage_type = "AzureFile"
    }
  }
  depends_on = [
    azurerm_storage_share_file.nginx_config,
    azurerm_container_app_environment_storage.nginx_configuration_storage
  ]
}