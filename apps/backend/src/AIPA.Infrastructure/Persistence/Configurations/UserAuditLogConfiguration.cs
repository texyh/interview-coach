using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for UserAuditLog entity
/// </summary>
public class UserAuditLogConfiguration : IEntityTypeConfiguration<UserAuditLog>
{
    public void Configure(EntityTypeBuilder<UserAuditLog> builder)
    {
        builder.ToTable("UserAuditLogs");

        // Primary key
        builder.HasKey(ual => ual.Id);

        // Configure UserAuditLogId value object
        builder.Property(ual => ual.Id)
            .HasConversion(
                id => id.Value,
                value => UserAuditLogId.From(value))
            .HasColumnName("Id");

        // Configure UserId value object
        builder.Property(ual => ual.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .HasColumnName("UserId");

        // Configure properties
        builder.Property(ual => ual.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ual => ual.EntityType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ual => ual.EntityId)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(ual => ual.OldValues)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(ual => ual.NewValues)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(ual => ual.Timestamp)
            .IsRequired();

        builder.Property(ual => ual.PerformedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(ual => ual.IpAddress)
            .HasMaxLength(45) // IPv6 addresses can be up to 45 characters
            .IsRequired();

        builder.Property(ual => ual.UserAgent)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ual => ual.AdditionalInfo)
            .HasColumnType("text")
            .IsRequired(false);

        // Create indexes
        builder.HasIndex(ual => ual.UserId)
            .HasDatabaseName("IX_UserAuditLogs_UserId");

        builder.HasIndex(ual => ual.Action)
            .HasDatabaseName("IX_UserAuditLogs_Action");

        builder.HasIndex(ual => ual.EntityType)
            .HasDatabaseName("IX_UserAuditLogs_EntityType");

        builder.HasIndex(ual => ual.Timestamp)
            .HasDatabaseName("IX_UserAuditLogs_Timestamp");

        builder.HasIndex(ual => ual.PerformedBy)
            .HasDatabaseName("IX_UserAuditLogs_PerformedBy")
            .HasFilter("\"PerformedBy\" IS NOT NULL");

        // Create composite indexes for common queries
        builder.HasIndex(ual => new { ual.UserId, ual.Timestamp })
            .HasDatabaseName("IX_UserAuditLogs_UserId_Timestamp");

        builder.HasIndex(ual => new { ual.EntityType, ual.EntityId, ual.Timestamp })
            .HasDatabaseName("IX_UserAuditLogs_EntityType_EntityId_Timestamp");

        // Configure audit fields from BaseEntity
        builder.Property(ual => ual.CreatedAt)
            .IsRequired();

        builder.Property(ual => ual.UpdatedAt)
            .IsRequired(false);

        // Configure relationships
        builder.HasOne(ual => ual.User)
            .WithMany() // No navigation property in User entity
            .HasForeignKey(ual => ual.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (they shouldn't be persisted)
        builder.Ignore(ual => ual.DomainEvents);
    }
}
