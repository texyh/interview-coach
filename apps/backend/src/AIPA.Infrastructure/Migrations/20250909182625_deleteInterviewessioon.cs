using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIPA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class deleteInterviewessioon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropTable(
            //     name: "SessionEvent");

            migrationBuilder.DropTable(
                name: "InterviewSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Difficulty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PausedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    History_Analytics_ActiveTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    History_Analytics_AverageResponseTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    History_Analytics_FirstResponseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    History_Analytics_LastResponseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    History_Analytics_OverallScore = table.Column<double>(type: "double precision", nullable: false),
                    History_Analytics_PauseCount = table.Column<int>(type: "integer", nullable: false),
                    History_Analytics_PausedTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    History_Analytics_PerformanceMetrics = table.Column<string>(type: "text", nullable: false),
                    History_Analytics_QuestionsAnswered = table.Column<int>(type: "integer", nullable: false),
                    History_Analytics_QuestionsAsked = table.Column<int>(type: "integer", nullable: false),
                    History_Analytics_SkillScores = table.Column<string>(type: "text", nullable: false),
                    History_Analytics_TotalDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    RecordingMetadata_AudioFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecordingMetadata_AudioFileSize = table.Column<long>(type: "bigint", nullable: true),
                    RecordingMetadata_AudioFormat = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RecordingMetadata_IsEncrypted = table.Column<bool>(type: "boolean", nullable: true),
                    RecordingMetadata_Metadata = table.Column<string>(type: "text", nullable: true),
                    RecordingMetadata_RecordingCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordingMetadata_RecordingDuration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    RecordingMetadata_RecordingStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordingMetadata_StorageProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecordingMetadata_TranscriptFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecordingMetadata_VideoFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecordingMetadata_VideoFileSize = table.Column<long>(type: "bigint", nullable: true),
                    RecordingMetadata_VideoFormat = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // migrationBuilder.CreateTable(
            //     name: "SessionEvent",
            //     columns: table => new
            //     {
            //         InterviewSessionHistoryInterviewSessionId = table.Column<Guid>(type: "uuid", nullable: false),
            //         Id = table.Column<Guid>(type: "uuid", nullable: false),
            //         Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
            //         EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            //         Metadata = table.Column<string>(type: "text", nullable: false),
            //         Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_SessionEvent", x => new { x.InterviewSessionHistoryInterviewSessionId, x.Id });
            //         table.ForeignKey(
            //             name: "FK_SessionEvent_InterviewSessions_InterviewSessionHistoryInter~",
            //             column: x => x.InterviewSessionHistoryInterviewSessionId,
            //             principalTable: "InterviewSessions",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSessions_CurrentState",
                table: "InterviewSessions",
                column: "CurrentState");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSessions_ScheduledAt",
                table: "InterviewSessions",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSessions_Status",
                table: "InterviewSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSessions_UserId",
                table: "InterviewSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSessions_UserId_CurrentState",
                table: "InterviewSessions",
                columns: new[] { "UserId", "CurrentState" });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSessions_UserId_Status",
                table: "InterviewSessions",
                columns: new[] { "UserId", "Status" });
        }
    }
}
