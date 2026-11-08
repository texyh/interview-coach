using AIPA.Domain.Common.Exceptions;
using AIPA.Domain.Enums;
using AIPA.Domain.Interviews.Entities;
using AIPA.Domain.Interviews.Enums;
using AIPA.Domain.Interviews.Repositories;
using AIPA.Domain.Users.Entities;
using AIPA.Domain.Users.Repositories;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Interviews.Services;

public class InterviewSessionDomainService
{
    private readonly IUserRepository _userRepository;
    private readonly IInterviewSessionRepository _sessionRepository;

    public InterviewSessionDomainService(
        IUserRepository userRepository,
        IInterviewSessionRepository sessionRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
    }

    public async Task<InterviewSession> ScheduleInterviewAsync(
        UserId userId,
        InterviewType type,
        DateTime scheduledAt,
        CancellationToken cancellationToken = default)
    {
        // Verify user exists and can schedule interviews
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new EntityNotFoundException("User", userId);

        if (!user.CanScheduleInterview())
            throw new BusinessRuleValidationException("User cannot schedule interviews in current status");

        // Check for scheduling conflicts
        var userSessions = await _sessionRepository.GetByUserIdAndStatusAsync(
            userId, InterviewSessionStatus.Scheduled, cancellationToken);

        var hasConflict = userSessions.Any(session =>
            Math.Abs((session.ScheduledAt - scheduledAt).TotalMinutes) < 60);

        if (hasConflict)
            throw new BusinessRuleValidationException("Interview session conflicts with existing scheduled session");

        // Create the session
        var session = InterviewSession.Create(
            user,
            "Generated Interview Session", // Default title
            null, // No description
            scheduledAt,
            TimeSpan.FromHours(1), // Default duration
            type,
            DifficultyLevel.Intermediate); // Default difficulty

        return session;
    }

    public async Task<bool> CanStartSessionAsync(
        InterviewSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        return session?.CanStart() ?? false;
    }

    public static bool IsValidRescheduleTime(DateTime currentScheduledAt, DateTime newScheduledAt)
    {
        // Must be at least 2 hours in advance
        var minimumAdvanceTime = DateTime.UtcNow.AddHours(2);
        if (newScheduledAt <= minimumAdvanceTime)
            return false;

        // Must be different from current time
        if (Math.Abs((newScheduledAt - currentScheduledAt).TotalMinutes) < 15)
            return false;

        return true;
    }
}
