# TASK 07-01-02: Develop Container Apps Environment Module

**Task ID**: TASK-07-01-02
**Story**: STORY-07-01 Infrastructure as Code Setup
**Estimated Hours**: 4
**Priority**: High
**Assignee**: DevOps Engineer

## Task Description

Create a modular Bicep template for Azure Container Apps Environment that will host all containerized services for the AI Interview Preparation Assistant platform. This module must support networking, logging, and managed identity configuration.

## Acceptance Criteria

- [ ] Container Apps environment with networking configuration
- [ ] Log Analytics workspace integration
- [ ] Custom domain and certificate configuration
- [ ] Managed identity setup for container registry access
- [ ] Environment variables and secrets configuration

## Technical Implementation

### Module Template Structure

```bicep
// modules/container-apps-environment.bicep
@description('Environment name')
param environmentName string

@description('Azure region')
param location string = resourceGroup().location

@description('Resource prefix for naming')
param resourcePrefix string

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${resourcePrefix}-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: environmentName == 'prod' ? 90 : 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
  tags: {
    environment: environmentName
    service: 'logging'
  }
}

// Container Apps Environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: '${resourcePrefix}-containerenv'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    zoneRedundant: environmentName == 'prod' ? true : false
  }
  tags: {
    environment: environmentName
    service: 'container-apps'
  }
}

// Managed Identity for Container Registry Access
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${resourcePrefix}-identity'
  location: location
  tags: {
    environment: environmentName
    service: 'identity'
  }
}

// Outputs
output environmentId string = containerAppsEnvironment.id
output environmentName string = containerAppsEnvironment.name
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output managedIdentityId string = managedIdentity.id
output managedIdentityClientId string = managedIdentity.properties.clientId
```

## Subtasks

### 1. Log Analytics Workspace Configuration (1 hour)

- Create Log Analytics workspace resource
- Configure retention policies based on environment
- Set up workspace permissions and access controls
- Configure cost management and data export

### 2. Container Apps Environment Setup (1.5 hours)

- Create Container Apps managed environment
- Configure logging integration with Log Analytics
- Set up zone redundancy for production environments
- Configure networking and ingress settings

### 3. Managed Identity Configuration (1 hour)

- Create user-assigned managed identity
- Configure identity for container registry access
- Set up role assignments and permissions
- Document identity usage patterns

### 4. Environment-Specific Configuration (0.5 hours)

- Implement conditional logic for different environments
- Configure production-specific features (zone redundancy, etc.)
- Set up development environment optimizations
- Validate parameter handling and defaults

## Definition of Done

- [ ] Bicep module creates Container Apps environment successfully
- [ ] Log Analytics workspace properly integrated
- [ ] Managed identity configured with appropriate permissions
- [ ] Environment-specific configurations implemented
- [ ] Module outputs properly defined for downstream usage
- [ ] Template validates without errors
- [ ] Documentation updated with module usage instructions
- [ ] Code reviewed and approved by team

## Dependencies

- Azure subscription with Container Apps provider registered
- Log Analytics workspace requirements defined
- Managed identity permissions schema established
- Network configuration requirements clarified

## Resources

- [Azure Container Apps Documentation](https://docs.microsoft.com/en-us/azure/container-apps/)
- [Log Analytics Workspace Configuration](https://docs.microsoft.com/en-us/azure/azure-monitor/logs/workspace-design)
- [Managed Identity Best Practices](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)

---

**Task Owner**: DevOps Engineer
**Reviewer**: Cloud Architect
**Due Date**: End of Sprint Day 3
