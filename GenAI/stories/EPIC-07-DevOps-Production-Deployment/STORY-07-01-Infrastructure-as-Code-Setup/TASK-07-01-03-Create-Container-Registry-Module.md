# TASK 07-01-03: Create Container Registry Module

**Task ID**: TASK-07-01-03
**Story**: STORY-07-01 Infrastructure as Code Setup
**Estimated Hours**: 3
**Priority**: High
**Assignee**: DevOps Engineer

## Task Description

Implement a Bicep template for Azure Container Registry (ACR) with enterprise-grade security features including geo-replication, vulnerability scanning, and managed identity authentication for the AI Interview Preparation Assistant platform.

## Acceptance Criteria

- [ ] Premium tier ACR with geo-replication
- [ ] Admin user disabled, managed identity authentication
- [ ] Network access rules and private endpoint
- [ ] Vulnerability scanning enabled
- [ ] Retention policies for image cleanup

## Technical Implementation

### Container Registry Module

```bicep
// modules/container-registry.bicep
@description('Container registry name')
param registryName string

@description('Azure region')
param location string = resourceGroup().location

@description('Environment name')
param environmentName string

@description('Managed identity resource ID for ACR access')
param managedIdentityId string = ''

// Container Registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: registryName
  location: location
  sku: {
    name: environmentName == 'prod' ? 'Premium' : 'Standard'
  }
  properties: {
    adminUserEnabled: false
    policies: {
      quarantinePolicy: {
        status: 'enabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'enabled'
      }
      retentionPolicy: {
        days: environmentName == 'prod' ? 30 : 7
        status: 'enabled'
      }
    }
    encryption: {
      status: 'disabled'
    }
    dataEndpointEnabled: true
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
  }
  tags: {
    environment: environmentName
    service: 'container-registry'
  }
}

// Enable geo-replication for production
resource geoReplication 'Microsoft.ContainerRegistry/registries/replications@2023-07-01' = if (environmentName == 'prod') {
  parent: containerRegistry
  name: 'westus2'
  location: 'westus2'
  properties: {}
}

// Vulnerability scanning
resource vulnerabilityAssessment 'Microsoft.Security/assessments@2021-06-01' = {
  name: guid(containerRegistry.id, 'vulnerability-assessment')
  scope: containerRegistry
  properties: {
    resourceDetails: {
      source: 'Azure'
      id: containerRegistry.id
    }
    displayName: 'Container Registry Vulnerability Assessment'
    status: {
      code: 'Healthy'
    }
  }
}

// Role assignment for managed identity (AcrPull)
resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(managedIdentityId)) {
  name: guid(containerRegistry.id, managedIdentityId, '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: managedIdentityId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output registryId string = containerRegistry.id
output registryUrl string = containerRegistry.properties.loginServer
output registryName string = containerRegistry.name
```

## Subtasks

### 1. Basic Registry Configuration (1 hour)

- Create container registry resource with appropriate SKU
- Configure basic properties and policies
- Set up retention policies for image cleanup
- Configure data endpoint and network access

### 2. Security Configuration (1 hour)

- Disable admin user and configure managed identity auth
- Enable vulnerability scanning and quarantine policies
- Configure trust policies for content verification
- Set up network security rules

### 3. Production Features (0.5 hours)

- Implement geo-replication for production environments
- Configure premium tier features conditionally
- Set up encryption and advanced security features
- Optimize settings for cost in non-production environments

### 4. Role Assignments and Permissions (0.5 hours)

- Configure role assignments for managed identities
- Set up AcrPull permissions for container apps
- Document permission requirements
- Test authentication flows

## Definition of Done

- [ ] Container registry created with appropriate tier and configuration
- [ ] Security features enabled (vulnerability scanning, quarantine)
- [ ] Managed identity authentication configured
- [ ] Geo-replication set up for production environment
- [ ] Role assignments configured for service access
- [ ] Retention policies implemented for cost optimization
- [ ] Template validates and deploys successfully
- [ ] Code reviewed and approved by security team

## Dependencies

- Managed identity must be created first
- Network security requirements defined
- Geo-replication regions approved for production
- Security scanning requirements established

## Resources

- [Azure Container Registry Documentation](https://docs.microsoft.com/en-us/azure/container-registry/)
- [ACR Security Best Practices](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-best-practices)
- [Container Registry Geo-replication](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-geo-replication)

---

**Task Owner**: DevOps Engineer
**Reviewer**: Security Engineer
**Due Date**: End of Sprint Day 4
