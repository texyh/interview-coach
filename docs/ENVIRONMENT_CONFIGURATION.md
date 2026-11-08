# AIPA Environment Configuration & Secrets Management

## Overview

This document provides comprehensive guidance for configuring environment variables and managing secrets across all AIPA services. The system supports three environments: **Development**, **Staging**, and **Production**, each with specific security and configuration requirements.

## Table of Contents

- [Quick Start](#quick-start)
- [Environment Overview](#environment-overview)
- [Service Configuration](#service-configuration)
- [Azure Key Vault Integration](#azure-key-vault-integration)
- [Security Best Practices](#security-best-practices)
- [Troubleshooting](#troubleshooting)
- [Configuration Reference](#configuration-reference)

## Quick Start

### Automated Setup

Use the provided script to automatically generate environment configurations:

```bash
# Development environment
./scripts/setup-environment.sh development

# Staging environment
./scripts/setup-environment.sh staging

# Production environment
./scripts/setup-environment.sh production

# Validate existing configuration
./scripts/setup-environment.sh [environment] --validate-only
```

### Manual Setup

If you prefer manual configuration, create the following environment files:

```
apps/backend/.env.development
apps/ai-service/.env.development
apps/web/.env.development
apps/mobile/.env.development
```

## Environment Overview

### Development Environment

**Purpose**: Local development and testing
**Security Level**: Low (credentials in plain text)
**Features**: Debug tools enabled, verbose logging, hot reload

**Characteristics**:

- Local database connections
- Mock external services
- Disabled SSL requirements
- Extensive logging and debugging
- Permissive CORS policies

### Staging Environment

**Purpose**: Pre-production testing and validation
**Security Level**: Medium (mixed credential storage)
**Features**: Production-like configuration with testing flexibility

**Characteristics**:

- Azure-hosted databases
- Real external service connections
- SSL required
- Enhanced monitoring
- Limited debug features

### Production Environment

**Purpose**: Live application deployment
**Security Level**: High (Azure Key Vault integration)
**Features**: Maximum security, performance optimization

**Characteristics**:

- All secrets in Azure Key Vault
- Strict security headers
- Minimal logging
- Performance monitoring
- Zero debug features

## Service Configuration

### 1. AIPA Backend (.NET)

#### Configuration Files

- `appsettings.Development.json` - Development settings
- `appsettings.Staging.json` - Staging settings
- `appsettings.Production.json` - Production settings
- `Configuration/ApplicationOptions.cs` - Strongly-typed configuration classes
- `Extensions/ConfigurationExtensions.cs` - Configuration validation and setup

#### Key Configuration Areas

**Database Configuration**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=aipa_dev;...",
    "ReadOnlyConnection": "Server=localhost;Database=aipa_dev;..."
  }
}
```

**OpenAI Configuration**

```json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4o",
    "MaxTokens": 4000,
    "Temperature": 0.7
  }
}
```

**Azure Services**

```json
{
  "Azure": {
    "KeyVault": {
      "Enabled": true,
      "VaultUri": "https://your-keyvault.vault.azure.net/"
    },
    "ApplicationInsights": {
      "ConnectionString": "...",
      "Enabled": true
    }
  }
}
```

#### Configuration Validation

The backend includes comprehensive configuration validation:

```csharp
// Automatic validation on startup
services.AddApplicationConfiguration(configuration, environment);

// Manual validation
var validator = serviceProvider.GetService<IConfigurationValidator>();
var result = await validator.ValidateAsync();
```

### 2. AIPA AI Service (NestJS)

#### Configuration Files

- `config/development.config.ts` - Development configuration
- `config/staging.config.ts` - Staging configuration
- `config/production.config.ts` - Production configuration
- `config/index.ts` - Configuration manager with validation

#### Key Configuration Areas

**Application Settings**

```typescript
app: {
  name: 'AIPA AI Service',
  port: 3001,
  environment: 'development',
  debug: true,
  logLevel: 'debug'
}
```

**Database Configuration**

```typescript
database: {
  type: 'postgresql',
  host: 'localhost',
  port: 5432,
  username: 'aipa_dev',
  password: 'dev_password_123',
  database: 'aipa_ai_dev'
}
```

**OpenAI Integration**

```typescript
openai: {
  apiKey: 'sk-dev-key-placeholder',
  model: 'gpt-4o',
  maxTokens: 4000,
  temperature: 0.7,
  baseUrl: 'https://api.openai.com/v1'
}
```

#### Environment-Specific Features

**Development**

- Local database connections
- Debug logging enabled
- Hot reload support
- Swagger documentation enabled

**Production**

- Azure Key Vault integration
- Compressed responses
- Security headers
- Monitoring and metrics

### 3. AIPA Web App (Next.js)

#### Configuration Files

- `next.config.development.js` - Development configuration
- `next.config.staging.js` - Staging configuration
- `next.config.production.js` - Production configuration
- `next.config.dynamic.ts` - Dynamic configuration loader

#### Environment Variables

**Public Variables** (Available in browser)

```env
NEXT_PUBLIC_APP_NAME=AIPA Interview Preparation
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000/api
NEXT_PUBLIC_AI_SERVICE_URL=http://localhost:3001
NEXT_PUBLIC_ENABLE_VOICE_ANALYSIS=true
```

**Server-Side Variables** (Server-only)

```env
API_SECRET_KEY=your-secret-key
DATABASE_URL=postgresql://...
OPENAI_API_KEY=sk-...
```

#### Security Headers

Production configuration includes comprehensive security headers:

```javascript
headers: [
  {
    key: "Content-Security-Policy",
    value: "default-src 'self'; script-src 'self' 'unsafe-inline'...",
  },
  {
    key: "Strict-Transport-Security",
    value: "max-age=31536000; includeSubDomains; preload",
  },
];
```

### 4. AIPA Mobile App (Flutter)

#### Configuration Files

- `.env.development` - Development environment variables
- `.env.staging` - Staging environment variables
- `.env.production` - Production environment variables

#### Key Variables

```env
# API Configuration
API_BASE_URL=http://localhost:5000/api
AI_SERVICE_URL=http://localhost:3001

# Feature Flags
ENABLE_VOICE_ANALYSIS=true
ENABLE_OFFLINE_MODE=true
ENABLE_PUSH_NOTIFICATIONS=false

# Platform Configuration
ANDROID_MIN_SDK_VERSION=21
IOS_MIN_VERSION=12.0
```

## Azure Key Vault Integration

### Production Secret Management

In production, all sensitive configuration is stored in Azure Key Vault and referenced using the `${secret-name}` syntax.

#### Setup Azure Key Vault

1. **Create Key Vault**

```bash
az keyvault create \
  --name aipa-prod-kv \
  --resource-group aipa-production \
  --location eastus
```

2. **Create Secrets**

```bash
az keyvault secret set --vault-name aipa-prod-kv --name "aipa-db-connection-string" --value "Server=..."
az keyvault secret set --vault-name aipa-prod-kv --name "openai-api-key" --value "sk-..."
az keyvault secret set --vault-name aipa-prod-kv --name "jwt-secret-key" --value "your-secret-key"
```

3. **Configure Access Policy**

```bash
az keyvault set-policy \
  --name aipa-prod-kv \
  --object-id <app-service-managed-identity-id> \
  --secret-permissions get list
```

#### Configuration Integration

**Backend (.NET)**

```json
{
  "Azure": {
    "KeyVault": {
      "Enabled": true,
      "VaultUri": "https://aipa-prod-kv.vault.azure.net/"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "${aipa-db-connection-string}"
  }
}
```

**AI Service (Node.js)**

```typescript
database: {
  host: process.env.AIPA_AI_DB_HOST || '${aipa-ai-db-host}',
  password: process.env.AIPA_AI_DB_PASSWORD || '${aipa-ai-db-password}'
}
```

### Secret Rotation

Implement automated secret rotation for enhanced security:

```bash
# Create rotation script
az keyvault secret set \
  --vault-name aipa-prod-kv \
  --name "secret-rotation-schedule" \
  --value "0 2 1 * *"  # Monthly rotation
```

## Security Best Practices

### 1. Environment Separation

- **Never** use production secrets in development/staging
- Maintain separate Azure Key Vaults per environment
- Use different service principal identities per environment
- Implement proper network isolation

### 2. Secret Management

**DO**:

- Store all production secrets in Azure Key Vault
- Use managed identities for Azure service authentication
- Implement secret rotation schedules
- Monitor secret access and usage
- Use short-lived tokens where possible

**DON'T**:

- Commit secrets to version control
- Share secrets via email or chat
- Use the same secrets across environments
- Store secrets in environment variables in production
- Use default or weak secrets

### 3. Configuration Validation

All services include configuration validation:

```typescript
// AI Service validation
const validationResult = validateConfiguration(config);
if (!validationResult.isValid) {
  console.error("Configuration errors:", validationResult.errors);
  process.exit(1);
}
```

```csharp
// Backend validation
services.AddOptions<ApplicationOptions>()
  .Bind(configuration)
  .ValidateDataAnnotations()
  .ValidateOnStart();
```

### 4. Monitoring and Alerting

Set up monitoring for configuration issues:

- Azure Key Vault access monitoring
- Configuration validation failures
- Secret rotation alerts
- Unauthorized access attempts

## Troubleshooting

### Common Issues

#### 1. Configuration Validation Errors

**Problem**: Service fails to start with configuration validation errors

**Solution**:

```bash
# Run validation
./scripts/setup-environment.sh [environment] --validate-only

# Check specific service configuration
cd apps/backend
dotnet run --environment Development
```

#### 2. Azure Key Vault Access Issues

**Problem**: Cannot access secrets from Key Vault

**Solution**:

```bash
# Check Azure CLI authentication
az account show

# Test Key Vault access
az keyvault secret show --vault-name aipa-prod-kv --name test-secret

# Verify managed identity permissions
az keyvault show --name aipa-prod-kv --query "properties.accessPolicies"
```

#### 3. Environment Loading Issues

**Problem**: Wrong environment configuration loaded

**Solution**:

```bash
# Check environment variables
echo $NODE_ENV
echo $ASPNETCORE_ENVIRONMENT

# Verify configuration files exist
ls -la apps/ai-service/config/
ls -la apps/backend/appsettings.*.json
```

#### 4. Database Connection Issues

**Problem**: Cannot connect to database

**Solution**:

```bash
# Test database connectivity
psql "Server=localhost;Database=aipa_dev;User Id=aipa_dev;Password=***"

# Check connection string format
# PostgreSQL: "Host=localhost;Database=aipa_dev;Username=aipa_dev;Password=***"
# SQL Server: "Server=localhost;Database=aipa_dev;User Id=aipa_dev;Password=***"
```

### Debug Mode

Enable debug mode for configuration troubleshooting:

**Backend**

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.Extensions.Configuration": "Debug"
    }
  }
}
```

**AI Service**

```typescript
monitoring: {
  logging: {
    level: 'debug',
    enableStructuredLogging: true
  }
}
```

## Configuration Reference

### Required Environment Variables by Service

#### Backend (.NET)

| Variable                               | Environment | Required | Description                 |
| -------------------------------------- | ----------- | -------- | --------------------------- |
| `ConnectionStrings__DefaultConnection` | All         | Yes      | Primary database connection |
| `OpenAI__ApiKey`                       | All         | Yes      | OpenAI API key              |
| `JWT__SecretKey`                       | All         | Yes      | JWT signing secret          |
| `Azure__KeyVault__VaultUri`            | Production  | Yes      | Key Vault URI               |

#### AI Service (NestJS)

| Variable             | Environment | Required | Description        |
| -------------------- | ----------- | -------- | ------------------ |
| `AIPA_AI_DB_HOST`    | All         | Yes      | Database host      |
| `OPENAI_API_KEY`     | All         | Yes      | OpenAI API key     |
| `JWT_SECRET_KEY`     | All         | Yes      | JWT signing secret |
| `AZURE_KEYVAULT_URI` | Production  | Yes      | Key Vault URI      |

#### Web App (Next.js)

| Variable                     | Environment | Required | Description      |
| ---------------------------- | ----------- | -------- | ---------------- |
| `NEXT_PUBLIC_API_BASE_URL`   | All         | Yes      | Backend API URL  |
| `NEXT_PUBLIC_AI_SERVICE_URL` | All         | Yes      | AI Service URL   |
| `NEXT_PUBLIC_ENVIRONMENT`    | All         | Yes      | Environment name |

#### Mobile App (Flutter)

| Variable         | Environment | Required | Description      |
| ---------------- | ----------- | -------- | ---------------- |
| `API_BASE_URL`   | All         | Yes      | Backend API URL  |
| `AI_SERVICE_URL` | All         | Yes      | AI Service URL   |
| `FLUTTER_ENV`    | All         | Yes      | Environment name |

### Azure Key Vault Secrets (Production)

| Secret Name                           | Description                     | Used By             |
| ------------------------------------- | ------------------------------- | ------------------- |
| `aipa-db-connection-string`           | Primary database connection     | Backend             |
| `aipa-ai-db-connection-string`        | AI service database connection  | AI Service          |
| `openai-api-key`                      | OpenAI API key                  | Backend, AI Service |
| `jwt-secret-key`                      | JWT signing secret              | All services        |
| `azure-speech-key`                    | Azure Speech Services key       | AI Service          |
| `azure-appinsights-connection-string` | Application Insights connection | All services        |

### Configuration File Locations

```
├── apps/backend/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── appsettings.Staging.json
│   ├── appsettings.Production.json
│   └── src/AIPA.WebApi/Configuration/
├── apps/ai-service/
│   └── config/
│       ├── index.ts
│       ├── development.config.ts
│       ├── staging.config.ts
│       └── production.config.ts
├── apps/web/
│   ├── next.config.ts
│   ├── next.config.development.js
│   ├── next.config.staging.js
│   └── next.config.production.js
└── scripts/
    └── setup-environment.sh
```

## Next Steps

1. **Review Configuration**: Examine the generated configuration files and update placeholder values
2. **Set Up Azure Key Vault**: Configure production secrets in Azure Key Vault
3. **Test Environments**: Validate each environment configuration
4. **Monitor and Alert**: Set up monitoring for configuration issues
5. **Document Custom Settings**: Add any project-specific configuration requirements

For additional support, refer to the service-specific README files or contact the development team.
