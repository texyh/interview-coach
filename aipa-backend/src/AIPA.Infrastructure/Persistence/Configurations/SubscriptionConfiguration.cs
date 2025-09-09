using AIPA.Domain.Enums;
using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for Subscription entity
/// </summary>
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        // Primary key
        builder.HasKey(s => s.Id);

        // Configure SubscriptionId value object
        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => SubscriptionId.From(value))
            .HasColumnName("Id");

        // Configure UserId value object
        builder.Property(s => s.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .HasColumnName("UserId");

        // Configure enums
        builder.Property(s => s.Tier)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.BillingInterval)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Configure properties
        builder.Property(s => s.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(s => s.StartDate)
            .IsRequired();

        builder.Property(s => s.EndDate)
            .IsRequired(false);

        builder.Property(s => s.NextBillingDate)
            .IsRequired(false);

        builder.Property(s => s.CancelledAt)
            .IsRequired(false);

        builder.Property(s => s.CancellationReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(s => s.ExternalSubscriptionId)
            .HasMaxLength(255)
            .IsRequired(false);

        // Configure UsageLimits owned entity
        builder.OwnsOne(s => s.UsageLimits, usageLimits =>
        {
            usageLimits.Property(ul => ul.MonthlyInterviewSessions)
                .HasColumnName("MonthlyInterviewSessions")
                .IsRequired();

            usageLimits.Property(ul => ul.MonthlyAIAnalysis)
                .HasColumnName("MonthlyAIAnalysis")
                .IsRequired();

            usageLimits.Property(ul => ul.StorageInMB)
                .HasColumnName("StorageInMB")
                .IsRequired();

            usageLimits.Property(ul => ul.CanAccessPremiumQuestions)
                .HasColumnName("CanAccessPremiumQuestions")
                .IsRequired();

            usageLimits.Property(ul => ul.CanExportData)
                .HasColumnName("CanExportData")
                .IsRequired();

            usageLimits.Property(ul => ul.HasPrioritySupport)
                .HasColumnName("HasPrioritySupport")
                .IsRequired();

            usageLimits.Property(ul => ul.MaxConcurrentSessions)
                .HasColumnName("MaxConcurrentSessions")
                .IsRequired();
        });

        // Create indexes
        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("IX_Subscriptions_UserId");

        builder.HasIndex(s => s.Status)
            .HasDatabaseName("IX_Subscriptions_Status");

        builder.HasIndex(s => s.Tier)
            .HasDatabaseName("IX_Subscriptions_Tier");

        builder.HasIndex(s => s.NextBillingDate)
            .HasDatabaseName("IX_Subscriptions_NextBillingDate");

        // Create unique constraint for external subscription ID (when present)
        builder.HasIndex(s => s.ExternalSubscriptionId)
            .IsUnique()
            .HasDatabaseName("IX_Subscriptions_ExternalSubscriptionId")
            .HasFilter("\"ExternalSubscriptionId\" IS NOT NULL");

        // Configure audit fields from BaseEntity
        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired(false);

        // Configure relationships
        builder.HasOne(s => s.User)
            .WithOne() // One-to-one relationship (user has one subscription)
            .HasForeignKey<Subscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.UsageHistory)
            .WithOne(u => u.Subscription)
            .HasForeignKey("SubscriptionId") // Shadow property
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events and computed properties
        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.IsActive);
        builder.Ignore(s => s.IsExpired);
    }
}
