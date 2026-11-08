using AIPA.Domain.Enums;

namespace AIPA.Application.DTOs.Users;

/// <summary>
/// User preferences update request DTO
/// </summary>
public class UpdateUserPreferencesRequest
{
    /// <summary>
    /// Whether to receive email notifications
    /// </summary>
    public bool EmailNotifications { get; set; }

    /// <summary>
    /// Whether to receive push notifications
    /// </summary>
    public bool PushNotifications { get; set; }

    /// <summary>
    /// Whether to receive weekly reports
    /// </summary>
    public bool WeeklyReports { get; set; }

    /// <summary>
    /// User's preferred language
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// User's preferred time zone
    /// </summary>
    public string TimeZone { get; set; } = "UTC";

    /// <summary>
    /// User's preferred theme
    /// </summary>
    public string Theme { get; set; } = "light";

    /// <summary>
    /// Interview types for which to receive notifications
    /// </summary>
    public List<InterviewType> NotificationInterviewTypes { get; set; } = [];

    /// <summary>
    /// Whether to share analytics
    /// </summary>
    public bool ShareAnalytics { get; set; }

    /// <summary>
    /// Whether to make profile public
    /// </summary>
    public bool PublicProfile { get; set; }

    /// <summary>
    /// Minutes before session to send reminder
    /// </summary>
    public int SessionReminderMinutes { get; set; } = 15;
}
