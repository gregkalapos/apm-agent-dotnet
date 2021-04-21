terraform {
	required_providers {
		azurerm = {
			source  = "hashicorp/azurerm"
			version = "=2.46.0"
		}
	}
}

provider "azurerm" {
	features {}
}

# configuration is sourced from the following environment variables:
# ARM_CLIENT_ID
# ARM_CLIENT_SECRET
# ARM_SUBSCRIPTION_ID
# ARM_TENANT_ID
# 
# See https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/guides/service_principal_client_secret
# for creating a Service Principal and Client Secret
data "azurerm_client_config" "current" {
}

variable "resource_group" {
	type = string
	description = "The name of the resource group to create"
}

variable "location" {
	type = string
	description = "The Azure location in which to deploy resources"
	default = "westus"
}

variable "failover_location" {
	type = string
	description = "The Azure location in which CosmosDb will failover"
	default = "westus2"
}

variable "cosmos_db_account_name" {
	type = string
	description = "The name of the cosmos db account to create"
}

resource "azurerm_resource_group" "cosmos_db_resource_group" {
  name     = var.resource_group
  location = var.location
}

resource "azurerm_cosmosdb_account" "cosmos_db_account" {
  name                = var.cosmos_db_account_name
  location            = azurerm_resource_group.cosmos_db_resource_group.location
  resource_group_name = azurerm_resource_group.cosmos_db_resource_group.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  enable_automatic_failover = true

  capabilities {
    name = "EnableAggregationPipeline"
  }

  capabilities {
    name = "mongoEnableDocLevelTTL"
  }

  capabilities {
    name = "MongoDBv3.4"
  }

  consistency_policy {
    consistency_level = "Strong"
  }

  geo_location {
    location          = var.failover_location
    failover_priority = 1
  }

  geo_location {
    location          = azurerm_resource_group.cosmos_db_resource_group.location
    failover_priority = 0
  }
}

resource "azurerm_cosmosdb_sql_database" "cosmosdb_sql_database" {
	name                = "${var.cosmos_db_account_name}_sql_database"
	resource_group_name = azurerm_cosmosdb_account.cosmos_db_account.resource_group_name
	account_name        = azurerm_cosmosdb_account.cosmos_db_account.name
	throughput          = 400
}

resource "azurerm_cosmosdb_sql_container" "cosmosdb_sql_container" {
	name                  = "${var.cosmos_db_account_name}_sql_container"
	resource_group_name   = azurerm_cosmosdb_account.cosmos_db_account.resource_group_name
	account_name          = azurerm_cosmosdb_account.cosmos_db_account.name
	database_name         = azurerm_cosmosdb_sql_database.cosmosdb_sql_database.name
	partition_key_path    = "/definition/id"
	partition_key_version = 1
	throughput            = 400

	indexing_policy {
		indexing_mode = "Consistent"

		included_path {
			path = "/*"
		}

		included_path {
			path = "/included/?"
		}

		excluded_path {
			path = "/excluded/?"
		}
	}

	unique_key {
		paths = ["/definition/idlong", "/definition/idshort"]
	}
}

output "endpoint" {
	value = azurerm_cosmosdb_account.cosmos_db_account.endpoint
}

output "primary_master_key" {
	value = azurerm_cosmosdb_account.cosmos_db_account.primary_master_key
	sensitive = true
}

output "database" {
	value = azurerm_cosmosdb_sql_database.cosmosdb_sql_database.name
}

output "container" {
	value = azurerm_cosmosdb_sql_container.cosmosdb_sql_container.name
}


