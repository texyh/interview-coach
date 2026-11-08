# EPIC 07: DevOps & Production Deployment

**Epic Priority**: 7 (Critical for Production Readiness)
**Phase**: 1 - MVP Core
**Estimated Duration**: 2-3 weeks
**Team Dependencies**: DevOps Engineer, Backend, Infrastructure

## Epic Overview

Establish robust DevOps practices and production deployment infrastructure to ensure reliable, scalable, and secure delivery of the AI Interview Preparation Assistant platform. This epic covers containerization, CI/CD pipelines, infrastructure as code, monitoring, and production environment setup.

## Business Value

- **Deployment Reliability**: Automated deployments reduce human error and downtime
- **Developer Productivity**: Streamlined development workflow increases team velocity
- **System Reliability**: Comprehensive monitoring ensures high availability and performance
- **Security Compliance**: Infrastructure security practices protect user data and platform integrity
- **Scalability Foundation**: Container orchestration enables horizontal scaling for growth
- **Cost Optimization**: Infrastructure automation reduces operational overhead

## Technical Scope

### Infrastructure as Code (IaC)

- **Azure Resource Provisioning**: Bicep templates for all Azure resources
- **Environment Management**: Separate dev, staging, and production environments
- **Resource Optimization**: Right-sized resources with auto-scaling capabilities
- **Network Security**: Virtual networks, security groups, and access controls
- **Database Management**: Azure Database for PostgreSQL with backup strategies

### Containerization & Orchestration

- **Docker Containerization**: All services containerized with optimized images
- **Azure Container Registry**: Secure container image storage and management
- **Azure Container Apps**: Serverless container orchestration with auto-scaling
- **Service Mesh**: Container communication and service discovery
- **Configuration Management**: Environment-specific configuration injection

### CI/CD Pipeline Implementation

- **Source Control Integration**: GitHub Actions with automated workflows
- **Build Automation**: Multi-stage builds with caching and optimization
- **Testing Integration**: Automated unit, integration, and E2E testing
- **Security Scanning**: Container vulnerability and code security analysis
- **Deployment Automation**: Blue-green deployments with rollback capabilities

### Monitoring & Observability

- **Application Performance Monitoring**: Azure Application Insights integration
- **Log Aggregation**: Centralized logging with Azure Log Analytics
- **Metrics Collection**: Custom metrics and performance indicators
- **Alerting System**: Proactive issue detection and notification
- **Health Checks**: Service health monitoring and automatic recovery

## Success Criteria

- [ ] Complete automated deployment pipeline from code commit to production
- [ ] Zero-downtime deployments with automatic rollback on failure
- [ ] Infrastructure provisioned and managed entirely through code
- [ ] Comprehensive monitoring with alerting for all critical services
- [ ] Container images optimized for size and security
- [ ] Development environments identical to production (dev-prod parity)
- [ ] Deployment time reduced to under 15 minutes for full stack
- [ ] 99.9% uptime SLA achieved through robust infrastructure

## Stories in this Epic

1. **[STORY-07-01] Infrastructure as Code Setup (Bicep)**
2. **[STORY-07-02] Azure Container Registry Configuration**
3. **[STORY-07-03] Docker Containerization for All Services**
4. **[STORY-07-04] Azure Container Apps Environment Setup**
5. **[STORY-07-05] GitHub Actions CI/CD Pipeline**
6. **[STORY-07-06] Environment Configuration Management**
7. **[STORY-07-07] Database Migration & Backup Strategy**
8. **[STORY-07-08] Monitoring & Logging Infrastructure**
9. **[STORY-07-09] Security Scanning & Compliance**
10. **[STORY-07-10] Load Testing & Performance Validation**
11. **[STORY-07-11] Production Deployment & Validation**
12. **[STORY-07-12] Disaster Recovery & Business Continuity**

## Dependencies & Risks

### Dependencies

- **Azure Subscription**: Production Azure subscription with appropriate quotas
- **GitHub Repository**: Source control with Actions enabled
- **Domain Registration**: Custom domain for production deployment
- **SSL Certificates**: TLS certificates for secure communications
- **External Services**: OpenAI API, Stripe, Azure Speech Services access

### Risks & Mitigations

- **Risk**: Complex container orchestration causes deployment failures
  - **Mitigation**: Thorough testing in staging environment and gradual rollout
- **Risk**: Infrastructure costs exceed budget projections
  - **Mitigation**: Resource monitoring and auto-scaling with cost alerts
- **Risk**: Security vulnerabilities in container images
  - **Mitigation**: Automated security scanning and regular base image updates
- **Risk**: Database migration issues during deployment
  - **Mitigation**: Database migration testing and rollback procedures

## Technical Architecture

### Infrastructure as Code Structure

```bicep
// main.bicep - Root infrastructure template
param environmentName string = 'dev'
param location string = resourceGroup().location

// Container Apps Environment
module containerAppsEnvironment 'modules/container-apps-environment.bicep' = {
  name: 'container-apps-env'
  params: {
    environmentName: environmentName
    location: location
  }
}

// Container Registry
module containerRegistry 'modules/container-registry.bicep' = {
  name: 'container-registry'
  params: {
    environmentName: environmentName
    location: location
  }
}

// Database
module database 'modules/postgresql.bicep' = {
  name: 'postgresql-db'
  params: {
    environmentName: environmentName
    location: location
    adminPassword: databasePassword
  }
}

// Application Services
module backendService 'modules/container-app.bicep' = {
  name: 'backend-service'
  params: {
    environmentName: environmentName
    serviceName: 'backend'
    containerImage: '${containerRegistry.outputs.registryUrl}/aipa-backend:latest'
    environmentId: containerAppsEnvironment.outputs.environmentId
    secrets: [
      {
        name: 'database-connection'
        value: database.outputs.connectionString
      }
    ]
  }
}

module aiService 'modules/container-app.bicep' = {
  name: 'ai-service'
  params: {
    environmentName: environmentName
    serviceName: 'ai-service'
    containerImage: '${containerRegistry.outputs.registryUrl}/aipa-ai-service:latest'
    environmentId: containerAppsEnvironment.outputs.environmentId
    secrets: [
      {
        name: 'openai-api-key'
        value: openAiApiKey
      }
    ]
  }
}
```

### Container App Configuration

```bicep
// modules/container-app.bicep
param environmentName string
param serviceName string
param containerImage string
param environmentId string
param secrets array = []

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: '${environmentName}-${serviceName}'
  location: location
  properties: {
    environmentId: environmentId
    configuration: {
      activeRevisionsMode: 'Multiple'
      maxInactiveRevisions: 3
      secrets: secrets
      registries: [
        {
          server: containerRegistryUrl
          identity: userAssignedIdentityId
        }
      ]
      ingress: {
        external: true
        targetPort: 80
        allowInsecure: false
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
      }
    }
    template: {
      containers: [
        {
          name: serviceName
          image: containerImage
          resources: {
            cpu: '0.5'
            memory: '1Gi'
          }
          env: [
            {
              name: 'ENVIRONMENT'
              value: environmentName
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
        rules: [
          {
            name: 'http-scale'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
    }
  }
}
```

### Docker Configuration

#### Backend Dockerfile

```dockerfile
# .NET Backend Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/AIPA.Api/AIPA.Api.csproj", "src/AIPA.Api/"]
COPY ["src/AIPA.Core/AIPA.Core.csproj", "src/AIPA.Core/"]
COPY ["src/AIPA.Infrastructure/AIPA.Infrastructure.csproj", "src/AIPA.Infrastructure/"]
RUN dotnet restore "src/AIPA.Api/AIPA.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/AIPA.Api"
RUN dotnet build "AIPA.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AIPA.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

ENTRYPOINT ["dotnet", "AIPA.Api.dll"]
```

#### AI Service Dockerfile

```dockerfile
# NestJS AI Service Dockerfile
FROM node:20-alpine AS base
WORKDIR /app
RUN addgroup -g 1001 -S nodejs
RUN adduser -S nextjs -u 1001

FROM base AS deps
COPY package.json package-lock.json ./
RUN npm ci --only=production && npm cache clean --force

FROM base AS build
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM base AS runtime
COPY --from=deps /app/node_modules ./node_modules
COPY --from=build /app/dist ./dist
COPY --from=build /app/package.json ./package.json

USER nextjs
EXPOSE 3001

CMD ["node", "dist/main"]
```

### CI/CD Pipeline Configuration

#### GitHub Actions Workflow

```yaml
# .github/workflows/deploy.yml
name: Deploy to Azure Container Apps

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  AZURE_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
  AZURE_RESOURCE_GROUP: ${{ secrets.AZURE_RESOURCE_GROUP }}
  CONTAINER_REGISTRY: ${{ secrets.CONTAINER_REGISTRY }}

jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service: [backend, ai-service, web-app]

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET (Backend)
        if: matrix.service == 'backend'
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "9.0.x"

      - name: Setup Node.js (AI Service & Web App)
        if: matrix.service != 'backend'
        uses: actions/setup-node@v4
        with:
          node-version: "20"
          cache: "npm"
          cache-dependency-path: "src/${{ matrix.service }}/package-lock.json"

      - name: Install dependencies and run tests
        run: |
          if [ "${{ matrix.service }}" = "backend" ]; then
            cd src/backend
            dotnet restore
            dotnet test --no-restore --verbosity normal
          else
            cd src/${{ matrix.service }}
            npm ci
            npm run test
          fi

  security-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Run Snyk security scan
        uses: snyk/actions/node@master
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
        with:
          args: --all-projects --severity-threshold=high

  build-and-push:
    needs: [test, security-scan]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main' || github.ref == 'refs/heads/develop'

    strategy:
      matrix:
        service: [backend, ai-service, web-app]

    steps:
      - uses: actions/checkout@v4

      - name: Azure Container Registry Login
        uses: azure/docker-login@v1
        with:
          login-server: ${{ env.CONTAINER_REGISTRY }}
          username: ${{ secrets.ACR_USERNAME }}
          password: ${{ secrets.ACR_PASSWORD }}

      - name: Build and push Docker image
        run: |
          IMAGE_TAG="${{ env.CONTAINER_REGISTRY }}/aipa-${{ matrix.service }}:${{ github.sha }}"
          docker build -t $IMAGE_TAG -f src/${{ matrix.service }}/Dockerfile .
          docker push $IMAGE_TAG

      - name: Tag as latest for main branch
        if: github.ref == 'refs/heads/main'
        run: |
          LATEST_TAG="${{ env.CONTAINER_REGISTRY }}/aipa-${{ matrix.service }}:latest"
          docker tag $IMAGE_TAG $LATEST_TAG
          docker push $LATEST_TAG

  deploy-infrastructure:
    needs: build-and-push
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'

    steps:
      - uses: actions/checkout@v4

      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Deploy Infrastructure
        uses: azure/arm-deploy@v1
        with:
          subscriptionId: ${{ env.AZURE_SUBSCRIPTION_ID }}
          resourceGroupName: ${{ env.AZURE_RESOURCE_GROUP }}
          template: ./infrastructure/main.bicep
          parameters: |
            environmentName=prod
            containerImages.backend=${{ env.CONTAINER_REGISTRY }}/aipa-backend:${{ github.sha }}
            containerImages.aiService=${{ env.CONTAINER_REGISTRY }}/aipa-ai-service:${{ github.sha }}
            containerImages.webApp=${{ env.CONTAINER_REGISTRY }}/aipa-web-app:${{ github.sha }}

  deploy-services:
    needs: deploy-infrastructure
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'

    steps:
      - name: Deploy Container Apps
        uses: azure/container-apps-deploy-action@v1
        with:
          containerAppName: prod-backend
          resourceGroup: ${{ env.AZURE_RESOURCE_GROUP }}
          imageToDeploy: ${{ env.CONTAINER_REGISTRY }}/aipa-backend:${{ github.sha }}

  health-check:
    needs: deploy-services
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'

    steps:
      - name: Health Check
        run: |
          echo "Waiting for services to be ready..."
          sleep 60

          # Check backend health
          curl -f https://aipa-backend.azurecontainerapps.io/health || exit 1

          # Check AI service health
          curl -f https://aipa-ai-service.azurecontainerapps.io/health || exit 1

          echo "All services are healthy!"
```

### Monitoring Configuration

#### Application Insights Setup

```typescript
// monitoring/application-insights.ts
import { ApplicationInsights } from "@azure/monitor-opentelemetry";

export class MonitoringService {
  private appInsights: ApplicationInsights;

  constructor(connectionString: string) {
    this.appInsights = new ApplicationInsights({
      connectionString,
      enableAutoCollectRequests: true,
      enableAutoCollectPerformance: true,
      enableAutoCollectExceptions: true,
      enableAutoCollectDependencies: true,
    });
  }

  trackCustomEvent(name: string, properties?: Record<string, any>): void {
    this.appInsights.trackEvent({
      name,
      properties: {
        timestamp: new Date().toISOString(),
        ...properties,
      },
    });
  }

  trackPerformanceMetric(
    name: string,
    value: number,
    properties?: Record<string, any>
  ): void {
    this.appInsights.trackMetric({
      name,
      value,
      properties,
    });
  }

  trackError(error: Error, properties?: Record<string, any>): void {
    this.appInsights.trackException({
      exception: error,
      properties,
    });
  }
}
```

#### Health Check Implementation

```csharp
// Health checks for .NET backend
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbContext _context;

    public DatabaseHealthCheck(IDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}

// Health check endpoint configuration
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database")
        .AddCheck<OpenAIHealthCheck>("openai-api")
        .AddCheck<AzureSpeechHealthCheck>("azure-speech");
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
}
```

### Environment Configuration Management

#### Azure Key Vault Integration

```csharp
// Configuration management with Key Vault
public class AzureKeyVaultConfigurationProvider : ConfigurationProvider
{
    private readonly SecretClient _secretClient;

    public AzureKeyVaultConfigurationProvider(string keyVaultUrl)
    {
        var credential = new DefaultAzureCredential();
        _secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
    }

    public override void Load()
    {
        var secrets = _secretClient.GetPropertiesOfSecretsAsync();

        await foreach (var secret in secrets)
        {
            var secretValue = await _secretClient.GetSecretAsync(secret.Name);
            Data[secret.Name.Replace("-", ":")] = secretValue.Value.Value;
        }
    }
}
```

#### Environment-Specific Configuration

```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "@Microsoft.KeyVault(SecretUri=https://aipa-kv.vault.azure.net/secrets/database-connection-string/)"
  },
  "OpenAI": {
    "ApiKey": "@Microsoft.KeyVault(SecretUri=https://aipa-kv.vault.azure.net/secrets/openai-api-key/)",
    "Model": "gpt-4"
  },
  "AzureSpeech": {
    "SubscriptionKey": "@Microsoft.KeyVault(SecretUri=https://aipa-kv.vault.azure.net/secrets/azure-speech-key/)",
    "Region": "eastus"
  },
  "Stripe": {
    "SecretKey": "@Microsoft.KeyVault(SecretUri=https://aipa-kv.vault.azure.net/secrets/stripe-secret-key/)",
    "WebhookSecret": "@Microsoft.KeyVault(SecretUri=https://aipa-kv.vault.azure.net/secrets/stripe-webhook-secret/)"
  },
  "ApplicationInsights": {
    "ConnectionString": "@Microsoft.KeyVault(SecretUri=https://aipa-kv.vault.azure.net/secrets/appinsights-connection-string/)"
  }
}
```

## Database Migration Strategy

### Migration Pipeline

```csharp
// Entity Framework migration strategy
public class DatabaseMigrationService
{
    private readonly IDbContext _context;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public async Task<bool> MigrateAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check if database exists
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                _logger.LogError("Cannot connect to database");
                return false;
            }

            // Get pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);

            if (pendingMigrations.Any())
            {
                _logger.LogInformation($"Applying {pendingMigrations.Count()} pending migrations");
                await _context.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database migration completed successfully");
            }
            else
            {
                _logger.LogInformation("No pending migrations found");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed");
            return false;
        }
    }
}
```

## Security Configuration

### Container Security Scanning

```yaml
# Security scanning in CI/CD
- name: Scan Docker image for vulnerabilities
  uses: aquasecurity/trivy-action@master
  with:
    image-ref: "${{ env.CONTAINER_REGISTRY }}/aipa-backend:${{ github.sha }}"
    format: "sarif"
    output: "trivy-results.sarif"

- name: Upload Trivy scan results to GitHub Security tab
  uses: github/codeql-action/upload-sarif@v2
  with:
    sarif_file: "trivy-results.sarif"
```

### Network Security

```bicep
// Network security group configuration
resource networkSecurityGroup 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: '${environmentName}-nsg'
  location: location
  properties: {
    securityRules: [
      {
        name: 'AllowHTTPS'
        properties: {
          priority: 1000
          protocol: 'Tcp'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: 'Internet'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '443'
        }
      }
      {
        name: 'AllowHTTP'
        properties: {
          priority: 1010
          protocol: 'Tcp'
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: 'Internet'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '80'
        }
      }
    ]
  }
}
```

## Performance Optimization

### Container Image Optimization

- **Multi-stage Builds**: Minimize final image size
- **Base Image Selection**: Use official, security-patched base images
- **Layer Caching**: Optimize Dockerfile for build cache efficiency
- **Security**: Non-root user execution and minimal attack surface

### Auto-scaling Configuration

```bicep
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  properties: {
    template: {
      scale: {
        minReplicas: 1
        maxReplicas: 20
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
          {
            name: 'cpu-utilization'
            custom: {
              type: 'cpu'
              metadata: {
                type: 'Utilization'
                value: '70'
              }
            }
          }
        ]
      }
    }
  }
}
```

## Disaster Recovery

### Backup Strategy

```bicep
// Automated database backups
resource postgresqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-06-01-preview' = {
  properties: {
    backup: {
      backupRetentionDays: 30
      geoRedundantBackup: 'Enabled'
    }
    highAvailability: {
      mode: 'ZoneRedundant'
    }
  }
}
```

### Rollback Procedures

```bash
#!/bin/bash
# Rollback script for Container Apps

RESOURCE_GROUP="aipa-prod-rg"
CONTAINER_APP="prod-backend"
PREVIOUS_REVISION="prod-backend--revision-suffix"

echo "Rolling back $CONTAINER_APP to revision $PREVIOUS_REVISION"

az containerapp revision activate \
  --resource-group $RESOURCE_GROUP \
  --name $CONTAINER_APP \
  --revision $PREVIOUS_REVISION

echo "Rollback completed. Verifying health..."
sleep 30

# Health check after rollback
curl -f https://aipa-backend.azurecontainerapps.io/health || {
  echo "Health check failed after rollback!"
  exit 1
}

echo "Rollback successful and service is healthy"
```

## Definition of Done

- [ ] Complete Infrastructure as Code templates provision all Azure resources
- [ ] All services containerized with optimized Docker images
- [ ] CI/CD pipeline automates build, test, security scan, and deployment
- [ ] Environment-specific configuration managed through Azure Key Vault
- [ ] Comprehensive monitoring and alerting configured for all services
- [ ] Database migration pipeline handles schema changes safely
- [ ] Security scanning integrated into build process with vulnerability thresholds
- [ ] Auto-scaling configured based on CPU and request metrics
- [ ] Health checks implemented for all services with automated recovery
- [ ] Disaster recovery procedures documented and tested
- [ ] Blue-green deployment capability with rollback procedures
- [ ] Production deployment achieves 99.9% uptime SLA target

---

**Epic Owner**: DevOps Engineer & Infrastructure Architect
**Stakeholders**: Development Team, Security Officer, Platform Owner
**Next Epic**: EPIC-08 Enhanced AI Features & Voice Processing
