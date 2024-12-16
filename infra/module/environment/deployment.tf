resource "azurerm_user_assigned_identity" "deployment_identity" {
  location            = "West Europe"
  name                = "${var.env}-deployment"
  resource_group_name = azurerm_resource_group.plant_based_pizza_rg.name
}

resource "azurerm_role_assignment" "role" {
  principal_id         = azurerm_user_assigned_identity.deployment_identity.principal_id
  role_definition_name = "Owner"
  scope                = azurerm_resource_group.plant_based_pizza_rg.id
}

resource "azurerm_role_assignment" "storage_blob_access_role" {
  principal_id         = azurerm_user_assigned_identity.deployment_identity.principal_id
  role_definition_name = "Storage Blob Data Contributor"
  scope                = data.azurerm_subscription.primary.id
}

resource "azurerm_federated_identity_credential" "deployment_identity_credentials" {
  name                = "${var.env}-deploy-creds"
  resource_group_name = azurerm_resource_group.plant_based_pizza_rg.name
  audience            = [local.default_audience_name]
  issuer              = local.github_issuer_url
  parent_id           = azurerm_user_assigned_identity.deployment_identity.id
  subject             = "repo:jeastham1993/PlantBasedPizza:ref:refs/heads/azure"
}
