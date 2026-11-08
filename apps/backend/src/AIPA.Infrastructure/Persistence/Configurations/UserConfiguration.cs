using AIPA.Domain.Enums;
using AIPA.Domain.Users.Entities;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for User entity
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // Primary key
        builder.HasKey(u => u.Id);

        // Configure UserId value object
        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .HasColumnName("Id");

        // Configure Email value object
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasColumnName("Email")
            .HasMaxLength(320) // RFC 5321 limit
            .IsRequired();

        // Create unique index for email
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        // Configure FullName value object
        builder.OwnsOne(u => u.FullName, fullName =>
        {
            fullName.Property(fn => fn.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            fullName.Property(fn => fn.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        // Configure UserProfile value object
        builder.OwnsOne(u => u.Profile, profile =>
        {
            profile.Property(p => p.JobTitle)
                .HasColumnName("JobTitle")
                .HasMaxLength(200)
                .IsRequired(false);

            profile.Property(p => p.Company)
                .HasColumnName("Company")
                .HasMaxLength(200)
                .IsRequired(false);

            profile.Property(p => p.YearsOfExperience)
                .HasColumnName("YearsOfExperience")
                .IsRequired(false);

            profile.Property(p => p.Skills)
                .HasColumnName("Skills")
                .HasConversion(
                    skills => string.Join(";", skills),
                    skillsString => skillsString.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(2000)
                .IsRequired(false);

            profile.Property(p => p.PreferredInterviewTypes)
                .HasColumnName("PreferredInterviewTypes")
                .HasConversion(
                    types => string.Join(";", types.Select(t => t.ToString())),
                    typesString => typesString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Enum.Parse<InterviewType>(s)).ToList())
                .HasMaxLength(500)
                .IsRequired(false);
        });

        // Configure UserPreferences value object
        builder.OwnsOne(u => u.Preferences, prefs =>
        {
            prefs.Property(p => p.EmailNotifications)
                .HasColumnName("EmailNotifications")
                .IsRequired();

            prefs.Property(p => p.PushNotifications)
                .HasColumnName("PushNotifications")
                .IsRequired();

            prefs.Property(p => p.WeeklyReports)
                .HasColumnName("WeeklyReports")
                .IsRequired();

            prefs.Property(p => p.Language)
                .HasColumnName("Language")
                .HasMaxLength(10)
                .IsRequired();

            prefs.Property(p => p.TimeZone)
                .HasColumnName("TimeZone")
                .HasMaxLength(50)
                .IsRequired();

            prefs.Property(p => p.Theme)
                .HasColumnName("Theme")
                .HasMaxLength(20)
                .IsRequired();

            prefs.Property(p => p.NotificationInterviewTypes)
                .HasColumnName("NotificationInterviewTypes")
                .HasConversion(
                    types => string.Join(";", types.Select(t => t.ToString())),
                    typesString => string.IsNullOrEmpty(typesString) ? new List<InterviewType>() :
                        typesString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => Enum.Parse<InterviewType>(s)).ToList())
                .HasMaxLength(500)
                .IsRequired();

            prefs.Property(p => p.ShareAnalytics)
                .HasColumnName("ShareAnalytics")
                .IsRequired();

            prefs.Property(p => p.PublicProfile)
                .HasColumnName("PublicProfile")
                .IsRequired();

            prefs.Property(p => p.SessionReminderMinutes)
                .HasColumnName("SessionReminderMinutes")
                .IsRequired();
        });

        // Configure AccountStatus enum
        builder.Property(u => u.Status)
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(50)
            .IsRequired();

        // Configure audit fields from BaseEntity
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired(false);

        // Configure relationships
        builder.HasMany(u => u.InterviewSessions)
            .WithOne(i => i.User)
            .HasForeignKey("UserId") // Shadow property
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (they shouldn't be persisted)
        builder.Ignore(u => u.DomainEvents);
    }
}
