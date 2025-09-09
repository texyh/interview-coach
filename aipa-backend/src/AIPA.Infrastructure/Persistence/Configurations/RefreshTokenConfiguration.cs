using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for RefreshToken entity
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        // Primary key
        builder.HasKey(rt => rt.Id);

        // Properties
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnName("Token");

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired()
            .HasColumnName("ExpiresAt");

        builder.Property(rt => rt.IsRevoked)
            .IsRequired()
            .HasColumnName("IsRevoked");

        builder.Property(rt => rt.RevokedAt)
            .HasColumnName("RevokedAt");

        builder.Property(rt => rt.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        builder.Property(rt => rt.UpdatedAt)
            .IsRequired()
            .HasColumnName("UpdatedAt");

        // Value object conversion for UserId
        builder.Property(rt => rt.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired()
            .HasColumnName("UserId");

        // Relationships
        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt");

        // Ignore computed properties
        builder.Ignore(rt => rt.IsActive);
    }
}
