using AIPA.Domain.Interviews.Entities;
using AIPA.Domain.Interviews.Enums;
using AIPA.Domain.Interviews.Repositories;
using AIPA.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AIPA.Infrastructure.Persistence.Repositories;

/// <summary>
/// Interview session repository implementation
/// </summary>
public class InterviewSessionRepository : BaseRepository<InterviewSession, InterviewSessionId>, IInterviewSessionRepository
{
    public InterviewSessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new async Task<InterviewSession?> GetByIdAsync(InterviewSessionId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public new async Task<IEnumerable<InterviewSession>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InterviewSession>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .Where(i => i.User.Id == userId)
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InterviewSession>> GetUpcomingSessionsAsync(DateTime fromDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .Where(i => i.ScheduledAt >= fromDate && i.Status == InterviewSessionStatus.Scheduled)
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InterviewSession>> GetSessionsByStatusAsync(InterviewSessionStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(InterviewSessionId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(i => i.Id == id, cancellationToken);
    }

    public new async Task AddAsync(InterviewSession session, CancellationToken cancellationToken = default)
    {
        await base.AddAsync(session, cancellationToken);
    }

    public async Task UpdateAsync(InterviewSession session, CancellationToken cancellationToken = default)
    {
        Update(session);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(InterviewSessionId id, CancellationToken cancellationToken = default)
    {
        var session = await GetByIdAsync(id, cancellationToken);
        if (session != null)
        {
            Remove(session);
        }
    }

    /// <summary>
    /// Get interview sessions with pagination and filtering
    /// </summary>
    public async Task<(IEnumerable<InterviewSession> Sessions, int TotalCount)> GetSessionsPagedAsync(
        int page,
        int pageSize,
        UserId? userId = null,
        InterviewSessionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(i => i.User).AsQueryable();

        // Apply filters
        if (userId != null)
        {
            query = query.Where(i => i.User.Id == userId);
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sessions = await query
            .OrderByDescending(i => i.ScheduledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (sessions, totalCount);
    }

    public async Task<IEnumerable<InterviewSession>> GetByUserIdAndStatusAsync(
        UserId userId,
        InterviewSessionStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .Where(i => i.User.Id == userId && i.Status == status)
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InterviewSession>> GetScheduledSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .Where(i => i.Status == InterviewSessionStatus.Scheduled)
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InterviewSession>> GetSessionsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .Where(i => i.ScheduledAt >= startDate && i.ScheduledAt <= endDate)
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<InterviewSession?> GetByIdAndUserAsync(InterviewSessionId id, UserId userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id && i.User.Id == userId, cancellationToken);
    }

    public async Task<IEnumerable<InterviewSession>> GetByUserIdAndStateAsync(UserId userId, InterviewSessionState state, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .Where(i => i.User.Id == userId && i.CurrentState == state)
            .OrderByDescending(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InterviewSession>> GetActiveSessionsByUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.User)
            .Where(i => i.User.Id == userId &&
                       (i.CurrentState == InterviewSessionState.InProgress ||
                        i.CurrentState == InterviewSessionState.Paused))
            .OrderByDescending(i => i.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CanUserAccessSessionAsync(InterviewSessionId id, UserId userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(i => i.Id == id && i.User.Id == userId, cancellationToken);
    }

    public async Task<int> GetActiveSessionCountForUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(i => i.User.Id == userId &&
                            (i.CurrentState == InterviewSessionState.InProgress ||
                             i.CurrentState == InterviewSessionState.Paused),
                       cancellationToken);
    }
}
