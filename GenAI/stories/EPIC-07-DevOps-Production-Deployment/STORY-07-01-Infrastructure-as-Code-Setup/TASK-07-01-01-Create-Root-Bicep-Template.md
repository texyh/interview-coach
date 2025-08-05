# TASK 07-01-01: Create Root Bicep Template

**Task ID**: TASK-07-01-01
**Story**: STORY-07-01 Infrastructure as Code Setup
**Estimated Hours**: 6
**Priority**: Critical
**Assignee**: DevOps Engineer

## Task Description

Develop the main.bicep template that serves as the root orchestrator for all infrastructure components in the AI Interview Preparation Assistant platform. This template will manage the deployment of all Azure resources with proper dependencies and configuration.

## Acceptance Criteria

- [ ] Main template includes all required Azure resources
- [ ] Parameter validation and default values configured
- [ ] Resource dependencies properly defined
- [ ] Template passes ARM validation
- [ ] Documentation includes deployment instructions

## Technical Implementation

### Template Structure

```bicep
// main.bicep - Root infrastructure template
targetScope = 'resourceGroup'

@description('Environment name (dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environmentName string = 'dev'

@description('Azure region for resource deployment')
param location string = resourceGroup().location

@description('Application name prefix')
param appName string = 'aipa'

@secure()
@description('Database administrator password')
param databasePassword string

@secure()
@description('OpenAI API key')
param openAiApiKey string

// Variables for resource naming
var resourcePrefix = '${appName}-${environmentName}'
var containerRegistryName = replace('${resourcePrefix}-acr', '-', '')
var keyVaultName = '${resourcePrefix}-kv'

// Container Apps Environment
module containerAppsEnvironment 'modules/container-apps-environment.bicep' = {
  name: 'container-apps-env-deployment'
  params: {
    environmentName: environmentName
    location: location
    resourcePrefix: resourcePrefix
  }
}

// Container Registry
module containerRegistry 'modules/container-registry.bicep' = {
  name: 'container-registry-deployment'
  params: {
    registryName: containerRegistryName
    location: location
    environmentName: environmentName
  }
}

// PostgreSQL Database
module database 'modules/postgresql.bicep' = {
  name: 'postgresql-deployment'
  params: {
    serverName: '${resourcePrefix}-postgres'
    location: location
    administratorPassword: databasePassword
    environmentName: environmentName
  }
}

// Key Vault
module keyVault 'modules/keyvault.bicep' = {
  name: 'keyvault-deployment'
  params: {
    keyVaultName: keyVaultName
    location: location
    environmentName: environmentName
  }
}

// Application Insights
module appInsights 'modules/app-insights.bicep' = {
  name: 'app-insights-deployment'
  params: {
    appInsightsName: '${resourcePrefix}-ai'
    location: location
    environmentName: environmentName
  }
}

// Storage Account
module storage 'modules/storage.bicep' = {
  name: 'storage-deployment'
  params: {
    storageAccountName: replace('${resourcePrefix}storage', '-', '')
    location: location
    environmentName: environmentName
  }
}

// Outputs for other deployments
output containerRegistryUrl string = containerRegistry.outputs.registryUrl
output containerAppsEnvironmentId string = containerAppsEnvironment.outputs.environmentId
output databaseConnectionString string = database.outputs.connectionString
output keyVaultName string = keyVault.outputs.keyVaultName
output appInsightsConnectionString string = appInsights.outputs.connectionString
```

## Subtasks

### 1. Template Structure Setup (1 hour)

- Create main.bicep file with basic structure
- Define parameter schema with validation
- Set up variable declarations for resource naming
- Configure target scope and metadata

### 2. Module Integration (2 hours)

- Reference all required Bicep modules
- Configure module parameters and dependencies
- Set up proper deployment sequencing
- Handle conditional deployments based on environment

### 3. Parameter Validation (1 hour)

- Implement parameter decorators and validation
- Set up default values for optional parameters
- Configure secure parameters for sensitive data
- Add parameter descriptions and constraints

### 4. Output Configuration (1 hour)

- Define outputs for downstream deployments
- Set up resource connection strings and URLs
- Configure secrets and managed identity outputs
- Document output usage and dependencies

### 5. Testing and Validation (1 hour)

- Run ARM template validation
- Test deployment in development environment
- Verify resource creation and configuration
- Document any deployment issues and resolutions

## Definition of Done

- [ ] Main.bicep template created with complete resource orchestration
- [ ] All parameters properly validated and documented
- [ ] Module references configured with correct dependencies
- [ ] Template passes ARM validation without errors or warnings
- [ ] Outputs properly configured for downstream consumption
- [ ] Deployment tested in development environment
- [ ] Code reviewed and approved by team
- [ ] Template committed to source control with version tag

## Dependencies

- Bicep modules for individual services must be created
- Resource naming conventions established
- Azure subscription permissions configured
- Parameter schema agreed upon by team

## Resources

- [Azure Bicep Documentation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [ARM Template Best Practices](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/best-practices)
- [Azure Resource Manager Deployment](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/deployment-models)

---

**Task Owner**: DevOps Engineer
**Reviewer**: Infrastructure Architect
**Due Date**: End of Sprint Day 2
