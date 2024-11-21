# We strongly recommend using the required_providers block to set the
# Azure Provider source and version being used
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.1.0"
    }
  }
  backend "azurerm" {
      resource_group_name  = "terraform-state"
      storage_account_name = "jeasthamterraformstate"
      container_name       = "plantbasedpizza"
      key                  = "orders.tfstate"
  }
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  resource_provider_registrations = "none"
  subscription_id = var.subscription_id
  features {}
}