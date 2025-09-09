using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for Role entity
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        // Primary key
        builder.HasKey(r => r.Id);

        // Configure RoleId value object
        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => RoleId.From(value))
            .HasColumnName("Id");

        // Configure properties
        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(r => r.IsActive)
            .IsRequired();

        // Create unique index for role name
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name");

        // Configure audit fields from BaseEntity
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired(false);

        // Configure relationships
        builder.HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "RolePermissions",
                j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                j => j.ToTable("RolePermissions")
            );

        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey("RoleId") // Shadow property
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (they shouldn't be persisted)
        builder.Ignore(r => r.DomainEvents);
    }
}
