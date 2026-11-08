using AIPA.Application.DTOs.Users;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIPA.Application.Users.Queries.GetUserPreferences;

/// <summary>
/// Handler for getting user preferences
/// </summary>
public class GetUserPreferencesQueryHandler : IRequestHandler<GetUserPreferencesQuery, UserPreferencesDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserPreferencesQueryHandler> _logger;

    public GetUserPreferencesQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserPreferencesQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserPreferencesDto> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting preferences for user: {UserId}", request.UserId);

        // Find user by ID
        var userId = UserId.From(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", request.UserId);
            throw new InvalidOperationException($"User with ID {request.UserId} not found");
        }

        _logger.LogInformation("Preferences retrieved successfully for user: {UserId}", request.UserId);

        // Return user preferences
        return new UserPreferencesDto
        {
            EmailNotifications = user.Preferences.EmailNotifications,
            PushNotifications = user.Preferences.PushNotifications,
            WeeklyReports = user.Preferences.WeeklyReports,
            Language = user.Preferences.Language,
            TimeZone = user.Preferences.TimeZone,
            Theme = user.Preferences.Theme,
            NotificationInterviewTypes = user.Preferences.NotificationInterviewTypes,
            ShareAnalytics = user.Preferences.ShareAnalytics,
            PublicProfile = user.Preferences.PublicProfile,
            SessionReminderMinutes = user.Preferences.SessionReminderMinutes
        };
    }
}
