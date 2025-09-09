using AIPA.Domain.Enums;
using AIPA.Domain.Interviews.Entities;
using AIPA.Domain.Interviews.Enums;
using AIPA.Domain.ValueObjects;

namespace AIPA.Domain.Interviews.Repositories;

public interface IInterviewSessionRepository
{
    Task<InterviewSession?> GetByIdAsync(InterviewSessionId id, CancellationToken cancellationToken = default);
    Task<InterviewSession?> GetByIdAndUserAsync(InterviewSessionId id, UserId userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewSession>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewSession>> GetByUserIdAndStatusAsync(
        UserId userId,
        InterviewSessionStatus status,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewSession>> GetByUserIdAndStateAsync(
        UserId userId,
        InterviewSessionState state,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewSession>> GetActiveSessionsByUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewSession>> GetScheduledSessionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewSession>> GetSessionsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
    Task<(IEnumerable<InterviewSession> Sessions, int TotalCount)> GetSessionsPagedAsync(
        int page,
        int pageSize,
        UserId? userId = null,
        InterviewSessionStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(InterviewSessionId id, CancellationToken cancellationToken = default);
    Task<bool> CanUserAccessSessionAsync(InterviewSessionId id, UserId userId, CancellationToken cancellationToken = default);
    Task<int> GetActiveSessionCountForUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(InterviewSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(InterviewSession session, CancellationToken cancellationToken = default);
    Task DeleteAsync(InterviewSessionId id, CancellationToken cancellationToken = default);
}
