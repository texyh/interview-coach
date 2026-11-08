using Serilog;
using Serilog.Events;

namespace AIPA.WebApi.Configuration;

/// <summary>
/// Configuration for structured logging with Serilog
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configure Serilog for the application
    /// </summary>
    /// <param name="builder">The web application builder</param>
    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        // Clear existing providers to use only Serilog
        builder.Logging.ClearProviders();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "AIPA")
            .Enrich.WithProperty("Version", typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{MachineName}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(
                path: "logs/aipa-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1),
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{MachineName}] [{EnvironmentName}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Debug)
            .WriteTo.File(
                path: "logs/aipa-errors-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 90,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1),
                restrictedToMinimumLevel: LogEventLevel.Warning)
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .CreateLogger();

        builder.Host.UseSerilog();

        Log.Information("AIPA API starting up...");
    }

    /// <summary>
    /// Configure additional Serilog settings from appsettings
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration to extend</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>LoggerConfiguration for chaining</returns>
    public static LoggerConfiguration ConfigureFromSettings(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        var logLevel = configuration["Logging:LogLevel:Default"] switch
        {
            "Trace" => LogEventLevel.Verbose,
            "Debug" => LogEventLevel.Debug,
            "Information" => LogEventLevel.Information,
            "Warning" => LogEventLevel.Warning,
            "Error" => LogEventLevel.Error,
            "Critical" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };

        return loggerConfiguration.MinimumLevel.Is(logLevel);
    }
}
