using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for SubscriptionUsage entity
/// </summary>
public class SubscriptionUsageConfiguration : IEntityTypeConfiguration<SubscriptionUsage>
{
    public void Configure(EntityTypeBuilder<SubscriptionUsage> builder)
    {
        builder.ToTable("SubscriptionUsage");

        // Primary key
        builder.HasKey(su => su.Id);

        // Configure SubscriptionUsageId value object
        builder.Property(su => su.Id)
            .HasConversion(
                id => id.Value,
                value => SubscriptionUsageId.From(value))
            .HasColumnName("Id");

        // Configure SubscriptionId value object
        builder.Property(su => su.SubscriptionId)
            .HasConversion(
                id => id.Value,
                value => SubscriptionId.From(value))
            .HasColumnName("SubscriptionId");

        // Configure properties
        builder.Property(su => su.FeatureUsed)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(su => su.Quantity)
            .IsRequired();

        builder.Property(su => su.UsedAt)
            .IsRequired();

        builder.Property(su => su.Metadata)
            .HasColumnType("text")
            .IsRequired(false);

        // Create indexes
        builder.HasIndex(su => su.SubscriptionId)
            .HasDatabaseName("IX_SubscriptionUsage_SubscriptionId");

        builder.HasIndex(su => su.FeatureUsed)
            .HasDatabaseName("IX_SubscriptionUsage_FeatureUsed");

        builder.HasIndex(su => su.UsedAt)
            .HasDatabaseName("IX_SubscriptionUsage_UsedAt");

        // Create composite index for subscription and feature
        builder.HasIndex(su => new { su.SubscriptionId, su.FeatureUsed })
            .HasDatabaseName("IX_SubscriptionUsage_SubscriptionId_FeatureUsed");

        // Create composite index for subscription and date
        builder.HasIndex(su => new { su.SubscriptionId, su.UsedAt })
            .HasDatabaseName("IX_SubscriptionUsage_SubscriptionId_UsedAt");

        // Configure audit fields from BaseEntity
        builder.Property(su => su.CreatedAt)
            .IsRequired();

        builder.Property(su => su.UpdatedAt)
            .IsRequired(false);

        // Configure relationships
        builder.HasOne(su => su.Subscription)
            .WithMany(s => s.UsageHistory)
            .HasForeignKey(su => su.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (they shouldn't be persisted)
        builder.Ignore(su => su.DomainEvents);
    }
}
