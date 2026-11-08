using Microsoft.Extensions.Diagnostics.HealthChecks;
using AIPA.Infrastructure.Persistence;
using AIPA.Application.Common.Interfaces;
using System.Diagnostics;
using HealthChecks.Uris;

namespace AIPA.WebApi.Configuration;

/// <summary>
/// Configuration for health checks including database and external service monitoring
/// </summary>
public static class HealthCheckConfiguration
{
    /// <summary>
    /// Add health check services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
    {

            services.AddHealthChecks().AddCheck<CustomHealthCheck>("system-resources",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready" })
            .AddCheck<MemoryHealthCheck>("memory-usage",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready" });

        // Add external service health checks if configured
        var aiServiceUrl = configuration.GetConnectionString("AIService");
        if (!string.IsNullOrEmpty(aiServiceUrl))
        {
            services.AddHealthChecks()
                .AddUrlGroup(
                    uri: new Uri(aiServiceUrl),
                    name: "ai-service",
                    failureStatus: HealthStatus.Degraded,
                    timeout: TimeSpan.FromSeconds(10),
                    tags: new[] { "external" });
        }

        return services;
    }
}

/// <summary>
/// Custom health check for system resources and application status
/// </summary>
public class CustomHealthCheck : IHealthCheck
{
    private readonly ILogger<CustomHealthCheck> _logger;

    public CustomHealthCheck(ILogger<CustomHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new Dictionary<string, object>();

            // Check disk space
            var driveInfo = new DriveInfo(Directory.GetCurrentDirectory());
            var freeSpaceGB = driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024);
            data.Add("diskFreeSpaceGB", freeSpaceGB);

            // Check if critical disk space threshold is reached
            if (freeSpaceGB < 1) // Less than 1GB free
            {
                _logger.LogWarning("Low disk space: {FreeSpace}GB remaining", freeSpaceGB);
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Critical disk space: {freeSpaceGB:F2}GB remaining",
                    data: data));
            }

            if (freeSpaceGB < 5) // Less than 5GB free
            {
                _logger.LogWarning("Low disk space warning: {FreeSpace}GB remaining", freeSpaceGB);
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Low disk space: {freeSpaceGB:F2}GB remaining",
                    data: data));
            }

            // Check environment variables
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            data.Add("environment", environment ?? "Unknown");

            // Check application uptime
            var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            data.Add("uptimeMinutes", uptime.TotalMinutes);

            return Task.FromResult(HealthCheckResult.Healthy(
                $"System healthy - Disk: {freeSpaceGB:F2}GB, Uptime: {uptime.TotalMinutes:F0}m",
                data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("System health check failed", ex));
        }
    }
}

/// <summary>
/// Health check specifically for memory usage monitoring
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly ILogger<MemoryHealthCheck> _logger;
    private const long MemoryThresholdUnhealthy = 500_000_000; // 500MB
    private const long MemoryThresholdDegraded = 200_000_000;  // 200MB

    public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryUsage = GC.GetTotalMemory(false);
            var memoryMB = memoryUsage / (1024.0 * 1024.0);

            var data = new Dictionary<string, object>
            {
                { "memoryUsageBytes", memoryUsage },
                { "memoryUsageMB", Math.Round(memoryMB, 2) },
                { "gen0Collections", GC.CollectionCount(0) },
                { "gen1Collections", GC.CollectionCount(1) },
                { "gen2Collections", GC.CollectionCount(2) }
            };

            if (memoryUsage > MemoryThresholdUnhealthy)
            {
                _logger.LogError("Critical memory usage: {MemoryMB:F2}MB", memoryMB);
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Critical memory usage: {memoryMB:F2}MB",
                    data: data));
            }

            if (memoryUsage > MemoryThresholdDegraded)
            {
                _logger.LogWarning("High memory usage: {MemoryMB:F2}MB", memoryMB);
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"High memory usage: {memoryMB:F2}MB",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"Memory usage normal: {memoryMB:F2}MB",
                data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Memory health check failed", ex));
        }
    }
}
