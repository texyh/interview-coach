using AIPA.Domain.Interviews.Entities;
using AIPA.Domain.Interviews.Enums;
using AIPA.Domain.Interviews.Repositories;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Interviews.Services;

public interface IInterviewSessionSecurityService
{
    Task<bool> CanUserAccessSessionAsync(InterviewSessionId sessionId, UserId userId, CancellationToken cancellationToken = default);
    Task<bool> CanUserModifySessionAsync(InterviewSessionId sessionId, UserId userId, CancellationToken cancellationToken = default);
    Task<bool> CanUserStartSessionAsync(InterviewSessionId sessionId, UserId userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateSessionStateTransitionAsync(InterviewSession session, InterviewSessionState targetState);
    Task<bool> HasUserReachedSessionLimitAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<bool> IsSessionExpiredAsync(InterviewSessionId sessionId, CancellationToken cancellationToken = default);
}

public class InterviewSessionSecurityService : IInterviewSessionSecurityService
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private const int MAX_ACTIVE_SESSIONS_PER_USER = 3;
    private const int SESSION_TIMEOUT_HOURS = 4;

    public InterviewSessionSecurityService(IInterviewSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
    }

    public async Task<bool> CanUserAccessSessionAsync(InterviewSessionId sessionId, UserId userId, CancellationToken cancellationToken = default)
    {
        return await _sessionRepository.CanUserAccessSessionAsync(sessionId, userId, cancellationToken);
    }

    public async Task<bool> CanUserModifySessionAsync(InterviewSessionId sessionId, UserId userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAndUserAsync(sessionId, userId, cancellationToken);
        if (session == null) return false;

        return session.CurrentState != InterviewSessionState.Completed &&
               session.CurrentState != InterviewSessionState.Cancelled &&
               !await IsSessionExpiredAsync(sessionId, cancellationToken);
    }

    public async Task<bool> CanUserStartSessionAsync(InterviewSessionId sessionId, UserId userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAndUserAsync(sessionId, userId, cancellationToken);
        if (session == null) return false;

        return session.CanStart() && !await HasUserReachedSessionLimitAsync(userId, cancellationToken);
    }

    public Task<bool> ValidateSessionStateTransitionAsync(InterviewSession session, InterviewSessionState targetState)
    {
        var isValidTransition = targetState switch
        {
            InterviewSessionState.InProgress => session.CurrentState == InterviewSessionState.NotStarted ||
                                               session.CurrentState == InterviewSessionState.Paused,
            InterviewSessionState.Paused => session.CurrentState == InterviewSessionState.InProgress,
            InterviewSessionState.Completed => session.CurrentState == InterviewSessionState.InProgress ||
                                              session.CurrentState == InterviewSessionState.Paused,
            InterviewSessionState.Cancelled => session.CurrentState != InterviewSessionState.Completed,
            _ => false
        };

        return Task.FromResult(isValidTransition);
    }

    public async Task<bool> HasUserReachedSessionLimitAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var activeSessionCount = await _sessionRepository.GetActiveSessionCountForUserAsync(userId, cancellationToken);
        return activeSessionCount >= MAX_ACTIVE_SESSIONS_PER_USER;
    }

    public async Task<bool> IsSessionExpiredAsync(InterviewSessionId sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session?.StartedAt == null) return false;

        var timeElapsed = DateTime.UtcNow - session.StartedAt.Value;
        return timeElapsed.TotalHours > SESSION_TIMEOUT_HOURS;
    }
}
