# TASK-01-01-05: Environment Configuration & Secrets Management

**Story**: STORY-01-01 Project Bootstrap & Repository Setup
**Epic**: EPIC-01 Foundation Infrastructure & Development Environment
**Estimated Hours**: 8 hours
**Assignee**: DevOps Engineer
**Sprint**: 1

## Task Description

Set up comprehensive environment configuration management including local development, staging, and production environments with proper secrets management and configuration validation.

## Acceptance Criteria

- [ ] Environment-specific configuration files created for all environments
- [ ] Azure Key Vault integration configured for production secrets
- [ ] Local development uses .env files with validation
- [ ] Configuration schema validation implemented
- [ ] Environment variables documented with examples
- [ ] Secrets rotation strategy documented

## Technical Implementation

### Environment Configuration Structure

```yaml
# appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=aipa_dev;Username=postgres;Password=dev_password"
  },
  "OpenAI": {
    "ApiKey": "local-development-key",
    "Model": "gpt-4o-mini"
  },
  "Azure": {
    "KeyVault": {
      "Enabled": false,
      "VaultUri": ""
    }
  }
}

# appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "@Microsoft.KeyVault(SecretUri=https://aipa-kv.vault.azure.net/secrets/db-connection-string/)"
  },
  "OpenAI": {
    "ApiKey": "@Microsoft.KeyVault(SecretUri=https://aipa-kv.vault.azure.net/secrets/openai-api-key/)",
    "Model": "gpt-4o"
  },
  "Azure": {
    "KeyVault": {
      "Enabled": true,
      "VaultUri": "https://aipa-kv.vault.azure.net/"
    }
  }
}
```

### Configuration Validation

```csharp
public class ConfigurationOptions
{
    [Required]
    public ConnectionStrings ConnectionStrings { get; set; }

    [Required]
    public OpenAISettings OpenAI { get; set; }

    public AzureSettings Azure { get; set; }
}

public class ConnectionStrings
{
    [Required]
    public string DefaultConnection { get; set; }
}
```

## Subtasks

1. **Environment Files Setup (2 hours)**

   - Create appsettings.json files for each environment
   - Set up .env file templates for local development
   - Configure environment-specific database connections

2. **Azure Key Vault Integration (3 hours)**

   - Configure Key Vault references in production settings
   - Set up managed identity for Key Vault access
   - Create secret naming conventions and documentation

3. **Configuration Validation (2 hours)**

   - Implement configuration options with data annotations
   - Add startup validation for required configuration
   - Create configuration health checks

4. **Documentation & Scripts (1 hour)**
   - Document all environment variables and their purposes
   - Create setup scripts for local environment configuration
   - Add troubleshooting guide for common configuration issues

## Definition of Done

- [ ] All environments have proper configuration files
- [ ] Azure Key Vault integration works in production
- [ ] Local development environment works without cloud dependencies
- [ ] Configuration validation prevents startup with invalid settings
- [ ] All secrets are properly managed and documented
- [ ] Configuration setup is automated and documented

## Dependencies

- Azure Key Vault provisioned and configured
- Environment-specific Azure resources available
- Development team onboarding requirements defined

## Resources

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Azure Key Vault Configuration Provider](https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)
- [Configuration Validation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
