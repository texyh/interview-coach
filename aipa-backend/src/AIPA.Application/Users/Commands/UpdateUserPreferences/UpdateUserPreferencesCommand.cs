using AIPA.Application.DTOs.Users;
using AIPA.Domain.Enums;
using MediatR;

namespace AIPA.Application.Users.Commands.UpdateUserPreferences;

/// <summary>
/// Command to update user preferences
/// </summary>
public record UpdateUserPreferencesCommand(
    Guid UserId,
    bool EmailNotifications,
    bool PushNotifications,
    bool WeeklyReports,
    string Language,
    string TimeZone,
    string Theme,
    List<InterviewType> NotificationInterviewTypes,
    bool ShareAnalytics,
    bool PublicProfile,
    int SessionReminderMinutes
) : IRequest<UserPreferencesDto>;
