using AIPA.Domain.Enums;

namespace AIPA.Domain.ValueObjects;

/// <summary>
/// Represents user preferences and settings
/// </summary>
public record UserPreferences
{
    public bool EmailNotifications { get; init; }
    public bool PushNotifications { get; init; }
    public bool WeeklyReports { get; init; }
    public string Language { get; init; }
    public string TimeZone { get; init; }
    public string Theme { get; init; } // "light", "dark", "auto"
    public List<InterviewType> NotificationInterviewTypes { get; init; }
    public bool ShareAnalytics { get; init; }
    public bool PublicProfile { get; init; }
    public int SessionReminderMinutes { get; init; }

    private UserPreferences(
        bool emailNotifications,
        bool pushNotifications,
        bool weeklyReports,
        string language,
        string timeZone,
        string theme,
        List<InterviewType> notificationInterviewTypes,
        bool shareAnalytics,
        bool publicProfile,
        int sessionReminderMinutes)
    {
        EmailNotifications = emailNotifications;
        PushNotifications = pushNotifications;
        WeeklyReports = weeklyReports;
        Language = language;
        TimeZone = timeZone;
        Theme = theme;
        NotificationInterviewTypes = notificationInterviewTypes ?? [];
        ShareAnalytics = shareAnalytics;
        PublicProfile = publicProfile;
        SessionReminderMinutes = sessionReminderMinutes;
    }

    public static UserPreferences CreateDefault() => new(
        emailNotifications: true,
        pushNotifications: true,
        weeklyReports: false,
        language: "en",
        timeZone: "UTC",
        theme: "auto",
        notificationInterviewTypes: [],
        shareAnalytics: false,
        publicProfile: false,
        sessionReminderMinutes: 15);

    public static UserPreferences Create(
        bool emailNotifications = true,
        bool pushNotifications = true,
        bool weeklyReports = false,
        string language = "en",
        string timeZone = "UTC",
        string theme = "auto",
        List<InterviewType>? notificationInterviewTypes = null,
        bool shareAnalytics = false,
        bool publicProfile = false,
        int sessionReminderMinutes = 15)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be empty", nameof(language));

        if (string.IsNullOrWhiteSpace(timeZone))
            throw new ArgumentException("TimeZone cannot be empty", nameof(timeZone));

        if (string.IsNullOrWhiteSpace(theme))
            throw new ArgumentException("Theme cannot be empty", nameof(theme));

        if (sessionReminderMinutes < 0 || sessionReminderMinutes > 1440) // Max 24 hours
            throw new ArgumentException("Session reminder must be between 0 and 1440 minutes", nameof(sessionReminderMinutes));

        var validThemes = new[] { "light", "dark", "auto" };
        if (!validThemes.Contains(theme.ToLowerInvariant()))
            throw new ArgumentException($"Theme must be one of: {string.Join(", ", validThemes)}", nameof(theme));

        return new UserPreferences(
            emailNotifications,
            pushNotifications,
            weeklyReports,
            language.Trim().ToLowerInvariant(),
            timeZone.Trim(),
            theme.Trim().ToLowerInvariant(),
            notificationInterviewTypes ?? [],
            shareAnalytics,
            publicProfile,
            sessionReminderMinutes);
    }

    public UserPreferences WithEmailNotifications(bool enabled) =>
        this with { EmailNotifications = enabled };

    public UserPreferences WithPushNotifications(bool enabled) =>
        this with { PushNotifications = enabled };

    public UserPreferences WithWeeklyReports(bool enabled) =>
        this with { WeeklyReports = enabled };

    public UserPreferences WithLanguage(string language) =>
        this with { Language = string.IsNullOrWhiteSpace(language) ? "en" : language.Trim().ToLowerInvariant() };

    public UserPreferences WithTimeZone(string timeZone) =>
        this with { TimeZone = string.IsNullOrWhiteSpace(timeZone) ? "UTC" : timeZone.Trim() };

    public UserPreferences WithTheme(string theme)
    {
        var validThemes = new[] { "light", "dark", "auto" };
        if (string.IsNullOrWhiteSpace(theme) || !validThemes.Contains(theme.ToLowerInvariant()))
            theme = "auto";

        return this with { Theme = theme.ToLowerInvariant() };
    }

    public UserPreferences WithNotificationInterviewTypes(List<InterviewType> types) =>
        this with { NotificationInterviewTypes = types ?? [] };

    public UserPreferences WithShareAnalytics(bool enabled) =>
        this with { ShareAnalytics = enabled };

    public UserPreferences WithPublicProfile(bool enabled) =>
        this with { PublicProfile = enabled };

    public UserPreferences WithSessionReminderMinutes(int minutes)
    {
        if (minutes < 0 || minutes > 1440)
            minutes = 15;

        return this with { SessionReminderMinutes = minutes };
    }
}
