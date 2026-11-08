using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for UserRole junction entity
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        // Primary key
        builder.HasKey(ur => ur.Id);

        // Configure UserRoleId value object
        builder.Property(ur => ur.Id)
            .HasConversion(
                id => id.Value,
                value => UserRoleId.From(value))
            .HasColumnName("Id");

        // Configure foreign key value objects
        builder.Property(ur => ur.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .HasColumnName("UserId");

        builder.Property(ur => ur.RoleId)
            .HasConversion(
                id => id.Value,
                value => RoleId.From(value))
            .HasColumnName("RoleId");

        // Configure properties
        builder.Property(ur => ur.AssignedAt)
            .IsRequired();

        builder.Property(ur => ur.ExpiresAt)
            .IsRequired(false);

        builder.Property(ur => ur.IsActive)
            .IsRequired();

        builder.Property(ur => ur.AssignedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(ur => ur.Reason)
            .HasMaxLength(500)
            .IsRequired(false);

        // Create composite index for UserId and RoleId
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasDatabaseName("IX_UserRoles_UserId_RoleId");

        // Create index for UserId
        builder.HasIndex(ur => ur.UserId)
            .HasDatabaseName("IX_UserRoles_UserId");

        // Create index for RoleId
        builder.HasIndex(ur => ur.RoleId)
            .HasDatabaseName("IX_UserRoles_RoleId");

        // Create index for IsActive
        builder.HasIndex(ur => ur.IsActive)
            .HasDatabaseName("IX_UserRoles_IsActive");

        // Configure audit fields from BaseEntity
        builder.Property(ur => ur.CreatedAt)
            .IsRequired();

        builder.Property(ur => ur.UpdatedAt)
            .IsRequired(false);

        // Configure relationships
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (they shouldn't be persisted)
        builder.Ignore(ur => ur.DomainEvents);
    }
}
