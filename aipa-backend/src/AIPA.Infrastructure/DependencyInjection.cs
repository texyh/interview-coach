using AIPA.Domain.Users.Repositories;
using AIPA.Domain.Interviews.Repositories;
using AIPA.Infrastructure.Persistence;
using AIPA.Application.Common.Interfaces;
using AIPA.Infrastructure.Persistence.Repositories;
using AIPA.Infrastructure.Persistence.Seeds;
using AIPA.Infrastructure.Repositories;
using AIPA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AIPA.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure layer services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Updated service collection</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            // Configure for development environment
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInterviewSessionRepository, AIPA.Infrastructure.Persistence.Repositories.InterviewSessionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Register Authentication Services
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IEmailService, EmailService>();

        // Register Email Service
        services.AddScoped<IEmailService, EmailService>();

        // Register Health Checks for Database
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready" });

        return services;
    }

    /// <summary>
    /// Ensures database is created and migrated
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task EnsureDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync(cancellationToken);

            // Apply any pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                await context.Database.MigrateAsync(cancellationToken);
            }

            // Seed initial data
            await DatabaseSeeder.SeedAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the exception here if logging is configured
            throw new InvalidOperationException("An error occurred while ensuring the database.", ex);
        }
    }
}
