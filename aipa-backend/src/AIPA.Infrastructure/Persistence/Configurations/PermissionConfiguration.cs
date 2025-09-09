using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for Permission entity
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        // Primary key
        builder.HasKey(p => p.Id);

        // Configure PermissionId value object
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => PermissionId.From(value))
            .HasColumnName("Id");

        // Configure properties
        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(p => p.Category)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        // Create unique index for permission name
        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("IX_Permissions_Name");

        // Create index for category
        builder.HasIndex(p => p.Category)
            .HasDatabaseName("IX_Permissions_Category");

        // Configure audit fields from BaseEntity
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);

        // Configure relationships
        builder.HasMany(p => p.Roles)
            .WithMany(r => r.Permissions)
            .UsingEntity<Dictionary<string, object>>(
                "RolePermissions",
                j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                j => j.ToTable("RolePermissions")
            );

        // Ignore domain events (they shouldn't be persisted)
        builder.Ignore(p => p.DomainEvents);
    }
}
