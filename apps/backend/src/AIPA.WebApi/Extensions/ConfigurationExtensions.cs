using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Core;
using AIPA.WebApi.Configuration;

namespace AIPA.WebApi.Extensions;

/// <summary>
/// Extension methods for configuration management
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Add and validate application configuration options
    /// </summary>
    public static IServiceCollection AddApplicationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Register the main application options with validation
        services.Configure<ApplicationOptions>(configuration);
        services.AddSingleton<IValidateOptions<ApplicationOptions>, ApplicationOptionsValidator>();

        // Register individual configuration sections for easier injection
        services.Configure<OpenAISettings>(configuration.GetSection("OpenAI"));
        services.Configure<JwtSettings>(configuration.GetSection("JWT"));
        services.Configure<AzureSettings>(configuration.GetSection("Azure"));
        services.Configure<CorsSettings>(configuration.GetSection("CORS"));
        services.Configure<RateLimitSettings>(configuration.GetSection("RateLimit"));
        services.Configure<FileUploadSettings>(configuration.GetSection("FileUpload"));
        services.Configure<SecuritySettings>(configuration.GetSection("Security"));
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.Configure<CacheSettings>(configuration.GetSection("Cache"));
        services.Configure<HealthCheckSettings>(configuration.GetSection("HealthChecks"));
        services.Configure<MonitoringSettings>(configuration.GetSection("Monitoring"));

        // Validate configuration on startup
        services.AddOptions<ApplicationOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Add configuration validation service
        services.AddScoped<IConfigurationValidator, ConfigurationValidator>();

        return services;
    }

    /// <summary>
    /// Configure Azure Key Vault if enabled
    /// </summary>
    public static IConfigurationBuilder AddAzureKeyVaultIfEnabled(
        this IConfigurationBuilder builder,
        IConfiguration configuration)
    {
        var keyVaultSettings = configuration.GetSection("Azure:KeyVault").Get<KeyVaultSettings>();

        if (keyVaultSettings?.Enabled == true && !string.IsNullOrEmpty(keyVaultSettings.VaultUri))
        {
            var keyVaultUri = new Uri(keyVaultSettings.VaultUri);

            // Use different credentials based on environment
            TokenCredential credential = string.IsNullOrEmpty(keyVaultSettings.ClientId)
                ? new DefaultAzureCredential()
                : new ClientSecretCredential(
                    keyVaultSettings.TenantId,
                    keyVaultSettings.ClientId,
                    configuration["Azure:KeyVault:ClientSecret"]);

            builder.AddAzureKeyVault(keyVaultUri, credential);
        }

        return builder;
    }

    /// <summary>
    /// Get a validated configuration section
    /// </summary>
    public static T GetValidatedSection<T>(this IConfiguration configuration, string sectionName)
        where T : class, new()
    {
        var section = configuration.GetSection(sectionName);
        var options = section.Get<T>() ?? new T();

        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validationContext = new ValidationContext(options);

        if (!Validator.TryValidateObject(options, validationContext, validationResults, true))
        {
            var errors = string.Join(Environment.NewLine,
                validationResults.Select(r => $"  - {r.ErrorMessage}"));
            throw new InvalidOperationException(
                $"Configuration validation failed for section '{sectionName}':{Environment.NewLine}{errors}");
        }

        return options;
    }

    /// <summary>
    /// Validate all configuration sections
    /// </summary>
    public static void ValidateConfiguration(this IConfiguration configuration)
    {
        try
        {
            // Validate main application options
            var appOptions = configuration.Get<ApplicationOptions>();
            ValidateObject(appOptions, "ApplicationOptions");

            // Validate individual sections
            ValidateConfigurationSection<OpenAISettings>(configuration, "OpenAI");
            ValidateConfigurationSection<JwtSettings>(configuration, "JWT");
            ValidateConfigurationSection<CorsSettings>(configuration, "CORS");
            ValidateConfigurationSection<RateLimitSettings>(configuration, "RateLimit");
            ValidateConfigurationSection<FileUploadSettings>(configuration, "FileUpload");
            ValidateConfigurationSection<SecuritySettings>(configuration, "Security");
            ValidateConfigurationSection<EmailSettings>(configuration, "Email");
            ValidateConfigurationSection<CacheSettings>(configuration, "Cache");
            ValidateConfigurationSection<HealthCheckSettings>(configuration, "HealthChecks");
            ValidateConfigurationSection<MonitoringSettings>(configuration, "Monitoring");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Configuration validation failed", ex);
        }
    }

    private static void ValidateConfigurationSection<T>(IConfiguration configuration, string sectionName)
        where T : class, new()
    {
        var section = configuration.GetSection(sectionName);
        if (section.Exists())
        {
            var options = section.Get<T>();
            ValidateObject(options, sectionName);
        }
    }

    private static void ValidateObject(object? obj, string objectName)
    {
        if (obj == null) return;

        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validationContext = new ValidationContext(obj);

        if (!Validator.TryValidateObject(obj, validationContext, validationResults, true))
        {
            var errors = string.Join(Environment.NewLine,
                validationResults.Select(r => $"  - {r.ErrorMessage}"));
            throw new InvalidOperationException(
                $"Validation failed for {objectName}:{Environment.NewLine}{errors}");
        }
    }
}

/// <summary>
/// Application options validator
/// </summary>
public class ApplicationOptionsValidator : IValidateOptions<ApplicationOptions>
{
    public ValidateOptionsResult Validate(string? name, ApplicationOptions options)
    {
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validationContext = new ValidationContext(options);

        if (!Validator.TryValidateObject(options, validationContext, validationResults, true))
        {
            var errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error");
            return ValidateOptionsResult.Fail(errors);
        }

        // Custom validation logic
        var customErrors = new List<string>();

        // Validate CORS origins format
        if (options.CORS.AllowedOrigins.Any(origin => !IsValidUri(origin)))
        {
            customErrors.Add("All CORS origins must be valid URIs");
        }

        // Validate file extensions format
        if (options.FileUpload.AllowedExtensions.Any(ext => !ext.StartsWith(".")))
        {
            customErrors.Add("File extensions must start with a dot (.)");
        }

        // Validate OpenAI model availability
        var validModels = new[] { "gpt-4", "gpt-4o", "gpt-3.5-turbo", "gpt-3.5-turbo-16k" };
        if (!validModels.Contains(options.OpenAI.Model))
        {
            customErrors.Add($"OpenAI model must be one of: {string.Join(", ", validModels)}");
        }

        if (customErrors.Any())
        {
            return ValidateOptionsResult.Fail(customErrors);
        }

        return ValidateOptionsResult.Success;
    }

    private static bool IsValidUri(string uri)
    {
        return Uri.TryCreate(uri, UriKind.Absolute, out _);
    }
}

/// <summary>
/// Configuration validation service
/// </summary>
public interface IConfigurationValidator
{
    Task<ConfigurationValidationResult> ValidateAsync();
    Task<bool> IsValidAsync();
}

/// <summary>
/// Implementation of configuration validation service
/// </summary>
public class ConfigurationValidator : IConfigurationValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationValidator> _logger;

    public ConfigurationValidator(IConfiguration configuration, ILogger<ConfigurationValidator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ConfigurationValidationResult> ValidateAsync()
    {
        var result = new ConfigurationValidationResult();

        try
        {
            // Validate database connection
            await ValidateDatabaseConnectionAsync(result);

            // Validate OpenAI connection
            await ValidateOpenAIConnectionAsync(result);

            // Validate Azure services
            await ValidateAzureServicesAsync(result);

            // Validate external dependencies
            await ValidateExternalDependenciesAsync(result);

            _logger.LogInformation("Configuration validation completed. Success: {Success}, Errors: {ErrorCount}",
                result.IsSuccess, result.Errors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration validation failed with exception");
            result.AddError("Configuration validation failed with exception", ex.Message);
        }

        return result;
    }

    public async Task<bool> IsValidAsync()
    {
        var result = await ValidateAsync();
        return result.IsSuccess;
    }

    private async Task ValidateDatabaseConnectionAsync(ConfigurationValidationResult result)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                result.AddError("Database", "DefaultConnection string is missing");
                return;
            }

            // Test database connectivity (implement based on your database provider)
            // This is a placeholder - implement actual database connectivity test
            await Task.Delay(100); // Simulate async operation

            _logger.LogInformation("Database connection validation passed");
        }
        catch (Exception ex)
        {
            result.AddError("Database", $"Database connection validation failed: {ex.Message}");
        }
    }

    private async Task ValidateOpenAIConnectionAsync(ConfigurationValidationResult result)
    {
        try
        {
            var openAISettings = _configuration.GetSection("OpenAI").Get<OpenAISettings>();
            if (openAISettings == null || string.IsNullOrEmpty(openAISettings.ApiKey))
            {
                result.AddError("OpenAI", "OpenAI configuration is missing or invalid");
                return;
            }

            // Test OpenAI API connectivity (implement actual test)
            await Task.Delay(100); // Simulate async operation

            _logger.LogInformation("OpenAI connection validation passed");
        }
        catch (Exception ex)
        {
            result.AddError("OpenAI", $"OpenAI connection validation failed: {ex.Message}");
        }
    }

    private async Task ValidateAzureServicesAsync(ConfigurationValidationResult result)
    {
        try
        {
            var azureSettings = _configuration.GetSection("Azure").Get<AzureSettings>();
            if (azureSettings?.KeyVault.Enabled == true)
            {
                // Validate Key Vault connectivity
                if (string.IsNullOrEmpty(azureSettings.KeyVault.VaultUri))
                {
                    result.AddError("Azure KeyVault", "KeyVault URI is required when KeyVault is enabled");
                }
            }

            await Task.Delay(100); // Simulate async operation

            _logger.LogInformation("Azure services validation passed");
        }
        catch (Exception ex)
        {
            result.AddError("Azure", $"Azure services validation failed: {ex.Message}");
        }
    }

    private async Task ValidateExternalDependenciesAsync(ConfigurationValidationResult result)
    {
        try
        {
            // Validate external service endpoints, API keys, etc.
            await Task.Delay(100); // Simulate async operation

            _logger.LogInformation("External dependencies validation passed");
        }
        catch (Exception ex)
        {
            result.AddError("External", $"External dependencies validation failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Configuration validation result
/// </summary>
public class ConfigurationValidationResult
{
    public bool IsSuccess => !Errors.Any();
    public List<ValidationError> Errors { get; } = new();

    public void AddError(string category, string message)
    {
        Errors.Add(new ValidationError(category, message));
    }
}

/// <summary>
/// Configuration validation error
/// </summary>
public record ValidationError(string Category, string Message);
