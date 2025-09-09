using AIPA.Domain.Common;
using AIPA.Domain.Interviews.Entities;
using AIPA.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

using AIPA.Infrastructure.Persistence;
using AIPA.Application.Common.Interfaces;

/// <summary>
/// Main application database context
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for main entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<InterviewSession> InterviewSessions { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Ignore entities that are not yet ready for EF mapping
        modelBuilder.Ignore<Role>();
        modelBuilder.Ignore<Permission>();
        modelBuilder.Ignore<UserRole>();
        modelBuilder.Ignore<Subscription>();
        modelBuilder.Ignore<SubscriptionUsage>();
        modelBuilder.Ignore<UserAuditLog>();

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Enable sensitive data logging in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Handle domain events before saving
        await DispatchDomainEventsAsync();

        // Update audit fields
        UpdateAuditableEntities();

        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync()
    {
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ToList()
            .ForEach(entity => entity.Entity.ClearDomainEvents());

        // Note: In a real implementation, you would use MediatR to publish these events
        // For now, we'll just clear them
        await Task.CompletedTask;
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
