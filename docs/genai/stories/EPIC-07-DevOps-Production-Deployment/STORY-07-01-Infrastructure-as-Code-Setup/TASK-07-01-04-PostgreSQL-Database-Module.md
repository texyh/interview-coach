# TASK 07-01-04: PostgreSQL Database Module

**Task ID**: TASK-07-01-04
**Story**: STORY-07-01 Infrastructure as Code Setup
**Estimated Hours**: 5
**Priority**: Critical
**Assignee**: DevOps Engineer

## Task Description

Create a comprehensive Bicep template for Azure Database for PostgreSQL Flexible Server with high availability, security, and backup configurations optimized for the AI Interview Preparation Assistant platform's data requirements.

## Acceptance Criteria

- [ ] Flexible server with high availability configuration
- [ ] Database firewall rules and private networking
- [ ] Automated backup configuration with retention
- [ ] Performance monitoring and alerting
- [ ] Database user and security configuration

## Technical Implementation

### PostgreSQL Module Template

```bicep
// modules/postgresql.bicep
@description('PostgreSQL server name')
param serverName string

@description('Azure region')
param location string = resourceGroup().location

@description('Environment name')
param environmentName string

@secure()
@description('Administrator password')
param administratorPassword string

@description('Administrator username')
param administratorUsername string = 'aipaadmin'

// PostgreSQL Flexible Server
resource postgresqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-06-01-preview' = {
  name: serverName
  location: location
  sku: {
    name: environmentName == 'prod' ? 'Standard_D4s_v3' : 'Standard_B2s'
    tier: environmentName == 'prod' ? 'GeneralPurpose' : 'Burstable'
  }
  properties: {
    version: '15'
    administratorLogin: administratorUsername
    administratorLoginPassword: administratorPassword
    storage: {
      storageSizeGB: environmentName == 'prod' ? 512 : 128
      autoGrow: 'Enabled'
    }
    backup: {
      backupRetentionDays: environmentName == 'prod' ? 35 : 7
      geoRedundantBackup: environmentName == 'prod' ? 'Enabled' : 'Disabled'
    }
    highAvailability: environmentName == 'prod' ? {
      mode: 'ZoneRedundant'
      standbyAvailabilityZone: '2'
    } : {}
    maintenanceWindow: {
      customWindow: 'Enabled'
      dayOfWeek: 0
      startHour: 2
      startMinute: 0
    }
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Enabled'
    }
  }
  tags: {
    environment: environmentName
    service: 'database'
  }
}

// Main application database
resource applicationDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  parent: postgresqlServer
  name: 'aipa_main'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// Firewall rule for Azure services
resource firewallRuleAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-06-01-preview' = {
  parent: postgresqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Diagnostic settings for monitoring
resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'database-diagnostics'
  scope: postgresqlServer
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        categoryGroup: 'allLogs'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: environmentName == 'prod' ? 90 : 30
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: environmentName == 'prod' ? 90 : 30
        }
      }
    ]
  }
}

// Outputs
output serverId string = postgresqlServer.id
output serverName string = postgresqlServer.name
output serverFqdn string = postgresqlServer.properties.fullyQualifiedDomainName
output connectionString string = 'Host=${postgresqlServer.properties.fullyQualifiedDomainName};Database=aipa_main;Username=${administratorUsername};Password=${administratorPassword};SSL Mode=Require;'
output databaseName string = applicationDatabase.name
```

## Subtasks

### 1. Server Configuration (2 hours)

- Configure PostgreSQL Flexible Server with appropriate SKU
- Set up version, authentication, and basic properties
- Configure storage with auto-grow capabilities
- Implement environment-specific sizing

### 2. High Availability and Backup (1.5 hours)

- Configure zone-redundant high availability for production
- Set up automated backup with appropriate retention
- Enable geo-redundant backup for production
- Configure maintenance windows

### 3. Security and Networking (1 hour)

- Configure firewall rules for Azure services
- Set up Azure AD authentication alongside password auth
- Implement network security best practices
- Configure SSL/TLS enforcement

### 4. Monitoring and Diagnostics (0.5 hours)

- Configure diagnostic settings for comprehensive logging
- Set up performance monitoring and metrics collection
- Configure alerts for database health and performance
- Enable query performance insights

## Definition of Done

- [ ] PostgreSQL Flexible Server deployed with correct configuration
- [ ] High availability configured for production environments
- [ ] Backup and retention policies implemented
- [ ] Security and authentication properly configured
- [ ] Monitoring and diagnostic settings enabled
- [ ] Database connection tested and validated
- [ ] Performance optimizations applied
- [ ] Code reviewed and approved by database team

## Dependencies

- Log Analytics workspace must exist for diagnostic settings
- Network configuration and firewall rules defined
- Database administrator credentials secured
- Performance requirements and sizing established

## Resources

- [Azure Database for PostgreSQL Documentation](https://docs.microsoft.com/en-us/azure/postgresql/)
- [PostgreSQL Flexible Server Best Practices](https://docs.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-best-practices)
- [Database Security Configuration](https://docs.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-security)

---

**Task Owner**: DevOps Engineer
**Reviewer**: Database Administrator
**Due Date**: End of Sprint Day 5
