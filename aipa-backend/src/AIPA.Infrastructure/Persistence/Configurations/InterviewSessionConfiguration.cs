using AIPA.Domain.Interviews.Entities;
using AIPA.Domain.Interviews.ValueObjects;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AIPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for InterviewSession entity
/// </summary>
public class InterviewSessionConfiguration : IEntityTypeConfiguration<InterviewSession>
{
    public void Configure(EntityTypeBuilder<InterviewSession> builder)
    {
        builder.ToTable("InterviewSessions");

        // Primary key
        builder.HasKey(i => i.Id);

        // Configure InterviewSessionId value object
        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => InterviewSessionId.From(value))
            .HasColumnName("Id");

        // Configure basic properties
        builder.Property(i => i.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasMaxLength(1000);

        builder.Property(i => i.ScheduledAt)
            .IsRequired();

        builder.Property(i => i.Duration)
            .IsRequired();

        // Configure Status enum
        builder.Property(i => i.Status)
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(50)
            .IsRequired();

        // Configure Type enum
        builder.Property(i => i.Type)
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(50)
            .IsRequired();

        // Configure Difficulty enum
        builder.Property(i => i.Difficulty)
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(50)
            .IsRequired();

        // Configure CurrentState enum
        builder.Property(i => i.CurrentState)
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(50)
            .IsRequired();

        // Configure timestamps
        builder.Property(i => i.StartedAt);
        builder.Property(i => i.PausedAt);
        builder.Property(i => i.ResumedAt);
        builder.Property(i => i.CompletedAt);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        // Configure RecordingMetadata complex object
        builder.OwnsOne(i => i.RecordingMetadata, recording =>
        {
            recording.Property(r => r.AudioFilePath).HasMaxLength(500);
            recording.Property(r => r.VideoFilePath).HasMaxLength(500);
            recording.Property(r => r.TranscriptFilePath).HasMaxLength(500);
            recording.Property(r => r.AudioFileSize);
            recording.Property(r => r.VideoFileSize);
            recording.Property(r => r.RecordingDuration);
            recording.Property(r => r.AudioFormat).HasMaxLength(10);
            recording.Property(r => r.VideoFormat).HasMaxLength(10);
            recording.Property(r => r.RecordingStartedAt);
            recording.Property(r => r.RecordingCompletedAt);
            recording.Property(r => r.IsEncrypted);
            recording.Property(r => r.StorageProvider).HasMaxLength(50);
            recording.Property(r => r.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null!) ?? new());
        });

        // Configure InterviewSessionHistory complex object (similar to RecordingMetadata)
        builder.OwnsOne(i => i.History, history =>
        {
            history.OwnsMany(h => h.Events, events =>
            {
                events.Property(e => e.Id).ValueGeneratedOnAdd();
                events.Property(e => e.Timestamp).IsRequired();
                events.Property(e => e.EventType).HasMaxLength(100).IsRequired();
                events.Property(e => e.Description).HasMaxLength(500).IsRequired();
                events.Property(e => e.Metadata)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new())
                    .HasColumnType("text");
            });

            history.OwnsOne(h => h.Analytics, analytics =>
            {
                analytics.Property(a => a.TotalDuration);
                analytics.Property(a => a.ActiveTime);
                analytics.Property(a => a.PausedTime);
                analytics.Property(a => a.PauseCount);
                analytics.Property(a => a.QuestionsAsked);
                analytics.Property(a => a.QuestionsAnswered);
                analytics.Property(a => a.OverallScore);
                analytics.Property(a => a.FirstResponseAt);
                analytics.Property(a => a.LastResponseAt);
                analytics.Property(a => a.AverageResponseTime);
                analytics.Property(a => a.SkillScores)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                        v => JsonSerializer.Deserialize<Dictionary<string, double>>(v, (JsonSerializerOptions)null!) ?? new())
                    .HasColumnType("text");
                analytics.Property(a => a.PerformanceMetrics)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new())
                    .HasColumnType("text");
            });
        });

        // Configure relationships
        builder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes
        builder.HasIndex(i => i.ScheduledAt);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.CurrentState);
        builder.HasIndex("UserId");
        builder.HasIndex("UserId", "Status");
        builder.HasIndex("UserId", "CurrentState");
    }
}
