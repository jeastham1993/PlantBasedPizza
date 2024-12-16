variable "subscription_id" {
  type = string
}

variable "resource_group_name"{
    type = string
}

variable "env" {
  type = string
}

variable "app_version" {
    type = string
}

variable "dd_site" {
    type = string
}

variable "dd_api_key" {
    type = string
}

variable "momento_api_key" {
  type = string
}

variable "cache_name" {
  type = string
  default = "plantbasedpizza.payments"
}

variable "db_connection_string" {
    type = string
}

variable "public_service_bus_namespace" {
    type = string
}