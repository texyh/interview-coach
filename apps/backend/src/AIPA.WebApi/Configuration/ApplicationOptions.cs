using System.ComponentModel.DataAnnotations;

namespace AIPA.WebApi.Configuration;

/// <summary>
/// Root configuration options with validation
/// </summary>
public class ApplicationOptions
{
    [Required(ErrorMessage = "ConnectionStrings configuration is required")]
    public ConnectionStrings ConnectionStrings { get; set; } = new();

    [Required(ErrorMessage = "OpenAI configuration is required")]
    public OpenAISettings OpenAI { get; set; } = new();

    [Required(ErrorMessage = "JWT configuration is required")]
    public JwtSettings JWT { get; set; } = new();

    public AzureSettings Azure { get; set; } = new();
    public CorsSettings CORS { get; set; } = new();
    public RateLimitSettings RateLimit { get; set; } = new();
    public FileUploadSettings FileUpload { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
    public CacheSettings Cache { get; set; } = new();
    public HealthCheckSettings HealthChecks { get; set; } = new();
    public MonitoringSettings Monitoring { get; set; } = new();
}

/// <summary>
/// Database connection strings configuration
/// </summary>
public class ConnectionStrings
{
    [Required(ErrorMessage = "DefaultConnection is required")]
    [MinLength(10, ErrorMessage = "DefaultConnection must be at least 10 characters")]
    public string DefaultConnection { get; set; } = string.Empty;

    public string ReadOnlyConnection { get; set; } = string.Empty;
}

/// <summary>
/// OpenAI service configuration
/// </summary>
public class OpenAISettings
{
    [Required(ErrorMessage = "OpenAI ApiKey is required")]
    [MinLength(20, ErrorMessage = "OpenAI ApiKey must be at least 20 characters")]
    public string ApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "OpenAI Model is required")]
    public string Model { get; set; } = "gpt-4o";

    [Range(1, 32000, ErrorMessage = "MaxTokens must be between 1 and 32000")]
    public int MaxTokens { get; set; } = 4000;

    [Range(0.0, 2.0, ErrorMessage = "Temperature must be between 0.0 and 2.0")]
    public double Temperature { get; set; } = 0.7;

    [Required(ErrorMessage = "BaseUrl is required")]
    [Url(ErrorMessage = "BaseUrl must be a valid URL")]
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
}

/// <summary>
/// JWT authentication configuration
/// </summary>
public class JwtSettings
{
    [Required(ErrorMessage = "JWT SecretKey is required")]
    [MinLength(32, ErrorMessage = "JWT SecretKey must be at least 32 characters for security")]
    public string SecretKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required")]
    [Url(ErrorMessage = "JWT Issuer must be a valid URL")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required")]
    [Url(ErrorMessage = "JWT Audience must be a valid URL")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "ExpiryMinutes must be between 1 and 1440 (24 hours)")]
    public int ExpiryMinutes { get; set; } = 60;

    [Range(1, 30, ErrorMessage = "RefreshTokenExpiryDays must be between 1 and 30")]
    public int RefreshTokenExpiryDays { get; set; } = 7;
}

/// <summary>
/// Azure services configuration
/// </summary>
public class AzureSettings
{
    public KeyVaultSettings KeyVault { get; set; } = new();
    public ApplicationInsightsSettings ApplicationInsights { get; set; } = new();
    public SpeechSettings Speech { get; set; } = new();
}

/// <summary>
/// Azure Key Vault configuration
/// </summary>
public class KeyVaultSettings
{
    public bool Enabled { get; set; } = false;

    public string VaultUri { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// Application Insights configuration
/// </summary>
public class ApplicationInsightsSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}

/// <summary>
/// Azure Speech Services configuration
/// </summary>
public class SpeechSettings
{
    public string SubscriptionKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Speech Region is required")]
    public string Region { get; set; } = "eastus";

    [Required(ErrorMessage = "Speech Language is required")]
    public string Language { get; set; } = "en-US";
}

/// <summary>
/// CORS policy configuration
/// </summary>
public class CorsSettings
{
    [Required(ErrorMessage = "At least one CORS origin must be specified")]
    [MinLength(1, ErrorMessage = "At least one CORS origin must be specified")]
    public List<string> AllowedOrigins { get; set; } = new();

    public bool AllowCredentials { get; set; } = true;
}

/// <summary>
/// Rate limiting configuration
/// </summary>
public class RateLimitSettings
{
    [Range(1, 1000, ErrorMessage = "RequestsPerMinute must be between 1 and 1000")]
    public int RequestsPerMinute { get; set; } = 100;

    [Range(1, 2000, ErrorMessage = "BurstSize must be between 1 and 2000")]
    public int BurstSize { get; set; } = 200;

    public bool Enabled { get; set; } = true;
}

/// <summary>
/// File upload configuration
/// </summary>
public class FileUploadSettings
{
    [Range(1, 100, ErrorMessage = "MaxFileSizeMB must be between 1 and 100")]
    public int MaxFileSizeMB { get; set; } = 10;

    [Required(ErrorMessage = "AllowedExtensions must be specified")]
    [MinLength(1, ErrorMessage = "At least one file extension must be allowed")]
    public List<string> AllowedExtensions { get; set; } = new();

    [Required(ErrorMessage = "StoragePath is required")]
    public string StoragePath { get; set; } = string.Empty;
}

/// <summary>
/// Security configuration
/// </summary>
public class SecuritySettings
{
    public bool EnableSecurityHeaders { get; set; } = true;
    public bool RequireHttps { get; set; } = false;

    [Required(ErrorMessage = "ContentSecurityPolicy is required")]
    public string ContentSecurityPolicy { get; set; } = string.Empty;
}

/// <summary>
/// Email service configuration
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SmtpPort must be between 1 and 65535")]
    public int SmtpPort { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "FromAddress is required")]
    [EmailAddress(ErrorMessage = "FromAddress must be a valid email")]
    public string FromAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "FromName is required")]
    public string FromName { get; set; } = string.Empty;
}

/// <summary>
/// Caching configuration
/// </summary>
public class CacheSettings
{
    public string RedisConnectionString { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "DefaultExpirationMinutes must be between 1 and 1440")]
    public int DefaultExpirationMinutes { get; set; } = 60;

    public bool Enabled { get; set; } = false;
}

/// <summary>
/// Health check configuration
/// </summary>
public class HealthCheckSettings
{
    public bool Enabled { get; set; } = true;

    [Range(1, 60, ErrorMessage = "DatabaseTimeout must be between 1 and 60 seconds")]
    public int DatabaseTimeout { get; set; } = 5;

    [Range(1, 60, ErrorMessage = "ExternalServiceTimeout must be between 1 and 60 seconds")]
    public int ExternalServiceTimeout { get; set; } = 10;
}

/// <summary>
/// Monitoring and telemetry configuration
/// </summary>
public class MonitoringSettings
{
    public bool EnableMetrics { get; set; } = true;
    public bool EnableTracing { get; set; } = true;

    [Range(0.0, 1.0, ErrorMessage = "SamplingRate must be between 0.0 and 1.0")]
    public double SamplingRate { get; set; } = 0.1;
}
