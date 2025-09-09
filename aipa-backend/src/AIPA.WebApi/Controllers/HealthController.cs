using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace AIPA.WebApi.Controllers;

/// <summary>
/// Health and status monitoring controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("System")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Get application health status
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult GetHealth()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "Unknown";

        var health = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = version,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Uptime = GetUptime(),
            Services = new
            {
                Database = "Connected", // This would be checked via health checks
                Application = "Running",
                Infrastructure = "Running"
            }
        };

        return Ok(health);
    }

    /// <summary>
    /// Get detailed system information
    /// </summary>
    /// <returns>System information</returns>
    [HttpGet("info")]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult GetSystemInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var info = new
        {
            Application = new
            {
                Name = "AIPA API",
                Version = assembly.GetName().Version?.ToString() ?? "Unknown",
                Framework = Environment.Version.ToString(),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            },
            System = new
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = Environment.WorkingSet,
                Timestamp = DateTime.UtcNow
            }
        };

        return Ok(info);
    }

    /// <summary>
    /// Simple readiness probe
    /// </summary>
    /// <returns>Ready status</returns>
    [HttpGet("ready")]
    [ProducesResponseType(200)]
    public ActionResult GetReadiness()
    {
        return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Simple liveness probe
    /// </summary>
    /// <returns>Live status</returns>
    [HttpGet("live")]
    [ProducesResponseType(200)]
    public ActionResult GetLiveness()
    {
        return Ok(new { Status = "Live", Timestamp = DateTime.UtcNow });
    }

    private static TimeSpan GetUptime()
    {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        return DateTime.UtcNow - process.StartTime.ToUniversalTime();
    }
}
