using AIPA.Application.Common.Interfaces;
using AIPA.Application.DTOs.Users;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIPA.Application.Users.Commands.UpdateUserPreferences;

/// <summary>
/// Handler for updating user preferences
/// </summary>
public class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommand, UserPreferencesDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserPreferencesCommandHandler> _logger;

    public UpdateUserPreferencesCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserPreferencesCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserPreferencesDto> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating preferences for user: {UserId}", request.UserId);

        // Find user by ID
        var userId = UserId.From(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", request.UserId);
            throw new InvalidOperationException($"User with ID {request.UserId} not found");
        }

        // Create new preferences
        var preferences = UserPreferences.Create(
            emailNotifications: request.EmailNotifications,
            pushNotifications: request.PushNotifications,
            weeklyReports: request.WeeklyReports,
            language: request.Language,
            timeZone: request.TimeZone,
            theme: request.Theme,
            notificationInterviewTypes: request.NotificationInterviewTypes,
            shareAnalytics: request.ShareAnalytics,
            publicProfile: request.PublicProfile,
            sessionReminderMinutes: request.SessionReminderMinutes);

        // Update user preferences
        user.UpdatePreferences(preferences);

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Preferences updated successfully for user: {UserId}", request.UserId);

        // Return updated preferences
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
